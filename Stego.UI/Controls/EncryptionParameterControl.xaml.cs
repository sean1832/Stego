using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NSec.Cryptography;
using Stego.Core;
using Stego.UI.Helpers;
using Stego.UI.ViewModel;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.XboxLive;
using Windows.Storage;

namespace Stego.UI.Controls;

public sealed partial class EncryptionParameterControl : UserControl
{
    public Argon2Parameters Argon2Param { get; private set; }
    private EncryptionPageViewModel? _vm;
    public EncryptionParameterControl()
    {
        InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        _vm = args.NewValue as EncryptionPageViewModel;
    }

    private async void ShowPasswordDialogClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_vm == null)
                return;

            // set argon2 parameters
            Argon2Param = new Argon2Parameters
            {
                DegreeOfParallelism = 1,
                MemorySize = _vm.Argon2Memory * 1024,
                NumberOfPasses = _vm.Argon2Cost
            };

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

            if (!string.IsNullOrEmpty(_vm.SteganographyViewModel.CoverImagePath))
            {
                // if cover image is selected, swap primary button to "Encrypt As File", disable secondary button
                dialog.PrimaryButtonText = "Encrypt As Image";
                dialog.PrimaryButtonClick += async (s, args) => await HandleEncryptClickAsync(dialog, prompt, args, SaveSteganographyFile);
            }
            else if (_vm.InputType != InputDataType.String && (_vm.Data != null) && (_vm.Data.Length >= 524288)) 
            {
                // if size larger than 0.5MB &&
                // if input type is file, swap primary button to "Encrypt As File", disable secondary button
                dialog.PrimaryButtonText = "Encrypt As File";
                dialog.PrimaryButtonClick += async (s, args) => await HandleEncryptClickAsync(dialog, prompt, args, SaveEncryptedFile);
            }
            else
            {
                dialog.PrimaryButtonText = "Encrypt Base64";
                dialog.SecondaryButtonText = "Encrypt As File";

                dialog.PrimaryButtonClick += async (s, args) => await HandleEncryptClickAsync(dialog, prompt, args, ShowB64Dialog);
                dialog.SecondaryButtonClick += async (s, args) => await HandleEncryptClickAsync(dialog, prompt, args, SaveEncryptedFile);
            }

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"Error showing password dialog during encryption: {ex.Message}");
        }
    }

    private async Task HandleEncryptClickAsync(
        ContentDialog dialog,
        PasswordPromptContent prompt,
        ContentDialogButtonClickEventArgs args,
        Action<byte[]> onSuccess)
    {
        args.Cancel = true;

        // validation
        if (string.IsNullOrWhiteSpace(prompt.Password))
        {
            prompt.ShowError("Password cannot be empty");
            return;
        }
        if (onSuccess == ShowB64Dialog && _vm!.SteganographyViewModel.CoverImagePath != null)
        {
            prompt.ShowError("Not support image output as base64");
            return;
        }

        // run encryption
        prompt.ShowSpinner();
        byte[] dataToEncrypt = _vm.Data;
        if (_vm.InputType != InputDataType.String)
        {
            if (string.IsNullOrEmpty(_vm.InputFilePath))
            {
                prompt.ShowError("No input file selected. Please select a file to encrypt.");
                return;
            }
            dataToEncrypt = await File.ReadAllBytesAsync(_vm.InputFilePath);
        }

        if (dataToEncrypt == null || dataToEncrypt.Length == 0)
        {
            prompt.ShowError("No data to encrypt. Please provide input data or select a file.");
            return;
        }

        byte[] data;
        try
        {
            data = await EncryptAsync(prompt.Password, dataToEncrypt);
        }
        catch (Exception e)
        {
            prompt.HideSpinner();
            prompt.ShowError($"Encryption failed: {e.Message}");
            return;
        }


        // done
        dialog.Hide();
        onSuccess(data);
    }

    private async void SaveSteganographyFile(byte[] encryptedData)
    {
        try
        {
            var opts = new FileSelectorSaveOptions
            {
                FileTypeChoices =
                {
                    { "Portable Network Graphics", [".png"] },
                    { "Bitmap", [".bmp"] }
                }
            };

            StorageFile? file = await FileSelector.PickSaveFileAsync(opts);
            if (file == null)
                return;

            await SpinnerDialogService.ShowWhileAsync(
                host: this,
                title: "Embedding data...",
                work: () => SteganographyLsb.EncodeAsync(
                    message: encryptedData,
                    coverFilePath: _vm!.SteganographyViewModel.CoverImagePath,
                    outputPath: file.Path,
                    spacing: _vm.SteganographyViewModel.Spacing,
                    lsbCount: _vm.SteganographyViewModel.LsbCount
                )
            );

            _vm!.IsOutputSuccess = true;
            _vm.OutputMessage = "GenericFile saved successfully.";
            _vm.OutputFilePath = file.Path;
        }
        catch (Exception e)
        {
            MessageBox.Error($"Error saving steganography image: {e.Message}");
        }
    }

    private async void SaveEncryptedFile(byte[] encryptedData)
    {
        try
        {
            var opts = new FileSelectorSaveOptions
            {
                FileTypeChoices =
                {
                    { "Stego GenericFile Format", [".stg"] },
                    { "Generic Binary", [".bin"] },
                    { "Portable Network Graphics", [".png"] },
                    { "Bitmap", [".bmp"] }
                }
            };

            var (success, file) = await SpinnerDialogService
                .ShowWhileAsync(
                    host: this,
                    title: "Saving file...",
                    work: () => FileSelector.SaveAsync(opts, encryptedData)
                );

            if (!success || file == null)
            {
                _vm!.IsOutputSuccess = false;
                _vm.OutputMessage = "Failed to save the file.";
            }
            else
            {
                _vm!.IsOutputSuccess = true;
                _vm.OutputMessage = "GenericFile saved successfully.";
                _vm.OutputFilePath = file.Path;
            }
        }
        catch (Exception e)
        {
            MessageBox.Error($"Error saving file: {e.Message}");
        }
    }

    private async void ShowB64Dialog(byte[] encryptedData)
    {
        try
        {
            OutputPromptContent outputPrompt = new OutputPromptContent();
            ContentDialog outputDialog = new ContentDialog
            {
                Title = "Encrypted Data",
                PrimaryButtonText = "Copy to Clipboard",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
                Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                Content = outputPrompt
            };

            outputPrompt.SetText(Convert.ToBase64String(encryptedData));
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
            MessageBox.Error($"Error showing base64 dialog: {e.Message}");
        }
    }

    private async Task<byte[]> EncryptAsync(string password, byte[] data)
    {
        // run the slow work off the UI thread
        byte[] pwBytes = Encoding.UTF8.GetBytes(password);
        return await Cipher.EncryptAes256GcmAsync(pwBytes, data, Argon2Param);
    }
}

