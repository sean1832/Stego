using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Text;
using Stego.UI.Helpers;
using Stego.UI.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.Controls
{
    public sealed partial class DecryptionInputControl : UserControl
    {
        private DecryptionPageViewModel? _vm;
        public DecryptionInputControl()
        {
            InitializeComponent();

            InputTypeComboBox.SelectionChanged += InputTypeComboBox_OnSelectionChanged;

            InputBox.Visibility = Visibility.Visible;
            FileSelector.Visibility = Visibility.Collapsed;

            this.DataContextChanged += DecryptionInputControl_DataContextChanged;
        }

        private void DecryptionInputControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _vm = args.NewValue as DecryptionPageViewModel;
        }

        private void InputTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (InputTypeComboBox.SelectedItem)
            {
                case "String":
                    _vm.InputType = InputDataType.String;
                    _vm.InputFilePath = null;
                    InputBox.Visibility = Visibility.Visible;
                    FileSelector.Visibility = Visibility.Collapsed;
                    break;
                case "File":
                    _vm.InputType = InputDataType.GenericFile;
                    _vm.Data = null;
                    InputBox.Visibility = Visibility.Collapsed;
                    FileSelector.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void FileSelector_OnSelectedFilePathChanged(object? sender, string? e)
        {
            if (_vm == null) return;
            if (string.IsNullOrEmpty(e)) return;

            // load all bytes from the file
            try
            {
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
            catch (Exception ex)
            {
                // Handle the exception (e.g., show a message to the user)
                Console.WriteLine($"Error reading file: {ex.Message}");
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
                return;
            }

            _vm.InputType = InputDataType.String;
            _vm.Data = data;
        }

        private static byte[]? GetInputBoxBytes(RichEditBox textBox)
        {
            textBox.Document.GetText(TextGetOptions.None, out string b64);
            // convert base64 string to byte array
            if (string.IsNullOrEmpty(b64)) return null;
            try
            {
                return Convert.FromBase64String(b64);
            }
            catch (FormatException)
            {
                // Handle the case where the string is not a valid base64 string
                MessageBox.Error("Failed to convert from base64 string.");
                return null;
            }
        }
    }
}
