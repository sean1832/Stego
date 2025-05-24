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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.Controls
{
    public sealed partial class MessageBoxContent : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(MessageBoxContent),
                new PropertyMetadata(string.Empty));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public MessageBoxContent()
        {
            InitializeComponent();
        }

        public void ShowError()
        {
            ErrorIcon.Visibility = Visibility.Visible;
            WarningIcon.Visibility = Visibility.Collapsed;
            InfoIcon.Visibility = Visibility.Collapsed;
        }
        public void ShowWarning()
        {
            ErrorIcon.Visibility = Visibility.Collapsed;
            WarningIcon.Visibility = Visibility.Visible;
            InfoIcon.Visibility = Visibility.Collapsed;
        }
        public void ShowInfo()
        {
            ErrorIcon.Visibility = Visibility.Collapsed;
            WarningIcon.Visibility = Visibility.Collapsed;
            InfoIcon.Visibility = Visibility.Visible;
        }
    }
}
