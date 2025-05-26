using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stego.Core;
using Stego.UI.Helpers;
using Stego.UI.ViewModel;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Stego.UI.Controls
{
    public sealed partial class DecryptionParameterControl : UserControl
    {
        private DecryptionPageViewModel? _vm;
        public DecryptionParameterControl()
        {
            InitializeComponent();
            this.DataContextChanged += DecryptionParameterControl_DataContextChanged;
        }

        private void DecryptionParameterControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _vm = args.NewValue as DecryptionPageViewModel;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_vm == null)
                    return;
                var prompt = new PasswordPromptContent();
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                    Title = "Password",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = prompt
                };

                switch (_vm.InputType)
                {
                    case InputDataType.GenericFile:
                        if (_vm.TextBoxData == null || _vm.TextBoxData.Length >= 524288)
                        {
                            dialog.PrimaryButtonText = "Decrypt As File";
                            dialog.PrimaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args,false, SaveDecryptedFile);
                            break;
                        }
                        dialog.PrimaryButtonText = "Decrypt As String";
                        dialog.PrimaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args,false, ShowTextOutputDialog);
                        dialog.SecondaryButtonText = "Decrypt As File";
                        dialog.SecondaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args,false, SaveDecryptedFile);
                        break;
                    case InputDataType.LosslessImage:
                        dialog.PrimaryButtonText = "Decrypt As String";
                        dialog.PrimaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args, true, ShowTextOutputDialog);
                        dialog.SecondaryButtonText = "Decrypt As File";
                        dialog.SecondaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args, true, SaveDecryptedFile);
                        break;
                    case InputDataType.JpegImage:
                        throw new NotImplementedException("Jpeg image is not currently supported");
                    case InputDataType.String:
                        dialog.PrimaryButtonText = "Decrypt As String";
                        dialog.PrimaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args, false, ShowTextOutputDialog);
                        dialog.SecondaryButtonText = "Decrypt As File";
                        dialog.SecondaryButtonClick += async (s, args) => await HandleDecryptionClickAsync(dialog, prompt, args, false, SaveDecryptedFile);
                        break;
                }

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Error showing password dialog during decryption: {ex.Message}");
            }
        }


        private async Task HandleDecryptionClickAsync(
            ContentDialog dialog,
            PasswordPromptContent prompt,
            ContentDialogButtonClickEventArgs args,
            bool isSteganographyFile,
            Func<byte[], Task> onSuccess)
        {
            if (_vm == null)
                throw new InvalidOperationException("ViewModel is not set.");

            args.Cancel = true;

            // validation
            if (string.IsNullOrWhiteSpace(prompt.Password))
            {
                prompt.ShowError("Password cannot be empty");
                return;
            }


            prompt.ShowSpinner();

            byte[]? data = _vm.TextBoxData;
            if ((data == null || data.Length == 0) && string.IsNullOrEmpty(_vm.InputFilePath))
            {
                prompt.HideSpinner();
                prompt.ShowError("No data to decrypt. Please provide input data or select a file.");
                return;
            }

            if (_vm.TextBoxData == null && !string.IsNullOrEmpty(_vm.InputFilePath))
            {
                data = await File.ReadAllBytesAsync(_vm.InputFilePath);
            }

            try
            {
                // decode steganography if needed
                if (isSteganographyFile)
                {
                    if (string.IsNullOrEmpty(_vm.InputFilePath))
                        throw new InvalidDataException($"{nameof(_vm.InputFilePath)} is not set.");
                    data = await SteganographyLsb.DecodeAsync(_vm.InputFilePath, (int)SpacingSlider.Value, (short)LsbControl.LsbCount);
                }

                if (data == null)
                {
                    throw new InvalidDataException("Decoded steganography data is null. Likely failed to decode.");
                }

                // decrypt
                byte[]? decrypted;
                try
                {
                    decrypted = await DecryptAsync(prompt.Password, data);
                }
                finally
                {
                    // clear data after decryption attempt
                    Array.Clear(data, 0, data.Length);
                    _vm.TextBoxData = null;
                }
                
                if (decrypted == null)
                {
                    throw new InvalidDataException("Decrypted data is null. Likely failed to decrypt.");
                }

                try
                {
                    // gzip
                    if (Compression.IsCompressedGz(decrypted))
                        decrypted = await Compression.DecompressGzAsync(decrypted);

                    dialog.Hide();
                    await onSuccess(decrypted);
                }
                finally
                {
                    // clear data after decryption attempt
                    Array.Clear(decrypted, 0, decrypted.Length);
                }
            }
            catch (Exception)
            {
                Fail();
            }
            return;

            void Fail()
            {
                prompt.HideSpinner();
                prompt.ShowError("Unable to process input. Please check your password or input data and try again.");
            }
        }

        private async Task ShowTextOutputDialog(byte[] decryptedData)
        {
            try
            {
                OutputPromptContent outputPrompt = new OutputPromptContent();
                ContentDialog outputDialog = new ContentDialog
                {
                    Title = "Decrypted Data",
                    PrimaryButtonText = "Copy to Clipboard",
                    CloseButtonText = "Close",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot,
                    Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                    Content = outputPrompt
                };

                outputPrompt.SetText(Encoding.UTF8.GetString(decryptedData));
                ContentDialogResult result = await outputDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    DataPackage dataPackage = new DataPackage();
                    dataPackage.SetText(outputPrompt.GetText());
                    Clipboard.SetContent(dataPackage);
                }
            }
            catch (Exception e)
            {
                MessageBox.Error($"Error showing text output: {e.Message}");
            }
        }

        private async Task SaveDecryptedFile(byte[] decryptedData)
        {
            try
            {
                var opts = new FileSelectorSaveOptions
                {
                    FileTypeChoices = {
                        { "Plaintext", [".txt"] },
                        { "PDF", [".pdf"] },
                        { "Zip archive", [".zip"] },
                        { "7z archive", [".7z"] },
                        { "Generic Binary", [".bin"] },
                    }
                };

                var (success, file) = await SpinnerDialogService
                    .ShowWhileAsync(
                        host: this,
                        title: "Saving file...",
                        work: () => FileSelector.SaveAsync(opts, decryptedData)
                    );

                // update VM
                if (!success || file == null)
                {
                    _vm!.IsOutputSuccess = false;
                    _vm.OutputMessage = "Failed to save the file.";
                }
                else
                {
                    _vm!.IsOutputSuccess = true;
                    _vm.OutputMessage = "File saved successfully.";
                    _vm.OutputFilePath = file.Path;
                }
            }
            catch (Exception e)
            {
                MessageBox.Error($"Error saving file: {e.Message}");
            }
        }

        private async Task<byte[]?> DecryptAsync(string password, byte[] data)
        {
            if (_vm == null) throw new InvalidOperationException("ViewModel is not set.");

            byte[] pwBytes = Encoding.UTF8.GetBytes(password);
            try
            {
                return await Cipher.DecryptAes256GcmAsync(pwBytes, data);
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., show a message to the user)
                MessageBox.Error($"Decryption Failed: {ex.Message}");
                return null;
            }
            finally
            {
                Array.Clear(pwBytes, 0, pwBytes.Length);
            }
        }

        private void SpacingSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SpacingSlider.Header = $"Spacing {SpacingSlider.Value}";
        }
    }
}
