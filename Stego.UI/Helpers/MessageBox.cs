using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Stego.UI.Controls;
using Stego.UI.View;

namespace Stego.UI.Helpers
{
    public class MessageBox
    {
        private static XamlRoot GetXamlRoot()
        {
            return ((FrameworkElement)App.MainWindow.Content).XamlRoot;
        }

        public static void Show(string message, string title, MessageBoxType type)
        {
            var window = new MessageBoxWindow
            {
                Message = message,
                Title = title,
            };
            window.SetType(type);
            window.Activate();
        }

        public static void Info(string message, string title="Info")
        {
            Show(message, title, MessageBoxType.Info);
        }
        public static void Warning(string message, string title = "Warning")
        {
            Show(message, title, MessageBoxType.Warning);
        }

        public static void Error(string message, string title = "Error")
        {
            Show(message, title, MessageBoxType.Error);
        }
    }
}
