using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text;
using Microsoft.UI.Text;
using Stego.Core;
using Stego.UI.Helpers;
using Stego.UI.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.Controls
{
    public sealed partial class EncryptionInputControl : UserControl
    {
        private EncryptionPageViewModel? _vm;

        public EncryptionInputControl()
        {
            InitializeComponent();

            InputTypeComboBox.SelectionChanged += InputTypeComboBox_OnSelectionChanged;
            TextInputPanel.Visibility = Visibility.Visible;
            FileSelectorControl.Visibility = Visibility.Collapsed;

            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _vm = args.NewValue as EncryptionPageViewModel;
        }

        private void InputTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (InputTypeComboBox.SelectedItem)
            {
                case "String":
                    _vm.InputType = InputDataType.String;
                    _vm.InputFilePath = null;
                    TextInputPanel.Visibility = Visibility.Visible;
                    FileSelectorControl.Visibility = Visibility.Collapsed;
                    break;
                case "File":
                    _vm.InputType = InputDataType.GenericFile;
                    _vm.Data = null;
                    TextInputPanel.Visibility = Visibility.Collapsed;
                    FileSelectorControl.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Triggered on InputBox clicked outside the box.
        /// </summary>
        private void InputBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
                return;

            byte[]? data = GetInputBoxBytes(InputBox);
            if (data == null)
            {
                // No data entered, clear the Data property
                _vm.Data = null;
                DataSizeTextBlock.Text = "0 bytes";
                return;
            }

            if (CompressToggle.IsOn)
            {
                data = Compression.CompressGz(data);
            }

            _vm.InputType = InputDataType.String;
            _vm.Data = data;
            DataSizeTextBlock.Text = $"{data.Length} bytes";
        }

        private static byte[]? GetInputBoxBytes(RichEditBox textBox)
        {
            textBox.Document.GetText(TextGetOptions.None, out string s);

            // strip off any paragraph breaks
            string trimmed = s.Trim('\r', '\n');
            if (string.IsNullOrEmpty(trimmed)) return null;

            return Encoding.UTF8.GetBytes(trimmed);
        }

        private void CompressToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            if (CompressToggle.IsOn)
            {
                if (_vm == null) return;

                byte[]? data = GetInputBoxBytes(InputBox);
                if (data == null) return;

                _vm.Data = Compression.CompressGz(data);
                DataSizeTextBlock.Text = $"{_vm.Data.Length} bytes";
            }
            else
            {
                if (_vm == null) return;

                byte[]? data = GetInputBoxBytes(InputBox);
                if (data == null) return;

                _vm.Data = data;
                DataSizeTextBlock.Text = $"{_vm.Data.Length} bytes";
            }
        }

        private void StatusBox_OnActionButtonClick(TeachingTip sender, object args)
        {
            if (_vm == null) return;

            // open directory
            string path = _vm.OutputFilePath;

            if (string.IsNullOrEmpty(path)) return;

            // open the directory in file explorer
            string? directoryPath = System.IO.Path.GetDirectoryName(path);
            if (directoryPath != null)
            {
                // Use the appropriate method to open the directory in file explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = directoryPath,
                    UseShellExecute = true
                });
            }

            StatusBox.IsOpen = false;
        }

        private void FileSelectorControl_OnSelectedFilePathChanged(object? sender, string? e)
        {
            if (_vm == null) return;
            if (string.IsNullOrEmpty(e))
            {
                _vm.Data = null;
                _vm.InputFilePath = null;
                return;
            }
            // store in input file path
            _vm.InputFilePath = e;
            if (e.EndsWith(".png") || e.EndsWith(".bmp"))
            {
                _vm.InputType = InputDataType.LosslessImage;
            }
            else if (e.EndsWith(".jpg") || e.EndsWith(".jpeg"))
            {
                _vm.InputType = InputDataType.JpegImage;
            }
            else
            {
                _vm.InputType = InputDataType.GenericFile;
            }
        }
    }
}
