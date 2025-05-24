using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NSec.Cryptography;
using Stego.Core;
using Stego.UI.Helpers;
using Stego.UI.ViewModel;
using System;
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
        else if (_vm.InputType == InputDataType.File && (_vm.Data != null) && (_vm.Data.Length >= 524288)) 
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
        byte[] data = await EncryptAsync(prompt.Password);

        // done
        dialog.Hide();
        onSuccess(data);
    }

    private async void SaveSteganographyFile(byte[] encryptedData)
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

        SteganographyLsb.Encode(
           encryptedData, 
           _vm.SteganographyViewModel.CoverImagePath, 
           file.Path, 
           _vm.SteganographyViewModel.Spacing
        );

        _vm!.IsOutputSuccess = true;
        _vm.OutputMessage = "File saved successfully.";
        _vm.OutputFilePath = file.Path;
    }

    private async void SaveEncryptedFile(byte[] encryptedData)
    {
        var opts = new FileSelectorSaveOptions
        {
            FileTypeChoices =
            {
                { "Stego File Format", [".stg"] },
                { "Generic Binary", [".bin"] },
                { "Portable Network Graphics", [".png"] },
                { "Bitmap", [".bmp"] }
            }
        };

        (bool success, StorageFile? file) = await FileSelector.SaveAsync(opts, encryptedData);
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

    private async void ShowB64Dialog(byte[] encryptedData)
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

    private async Task<byte[]> EncryptAsync(string password)
    {
        // run the slow work off the UI thread
        byte[] pwBytes = Encoding.UTF8.GetBytes(password);
        return await Task.Run(() =>
            Cipher.EncryptAes256Gcm(
                new ReadOnlySpan<byte>(pwBytes),
                _vm.Data,
                Argon2Param
            )
        );
    }
}

