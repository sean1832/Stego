using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Stego.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.View
{
    public enum MessageBoxType { Info, Warning, Error }
    public sealed partial class MessageBoxWindow : Window
    {

        // P/Invoke to get cursor position
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }


        public string Message { get; set; }
        public MessageBoxWindow()
        {
            InitializeComponent();

            AppWindow.Resize(new Windows.Graphics.SizeInt32(400, 200));
            AppWindow.SetIcon("Assets/Tiles/GalleryIcon.ico");
            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;

            AppWindow.SetPresenter(presenter);

            // After content is loaded, position at cursor
            CenterWindowAtCursor();
        }

        private void CenterWindowAtCursor()
        {
            if (!GetCursorPos(out var pt)) return;

            var halfW = AppWindow.Size.Width / 2;
            var halfH = AppWindow.Size.Height / 2;

            // Use the synchronous Move API
            AppWindow.Move(new PointInt32(pt.X - halfW, pt.Y - halfH));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void SetType(MessageBoxType type)
        {
            ErrorIcon.Visibility = type == MessageBoxType.Error ? Visibility.Visible : Visibility.Collapsed;
            WarningIcon.Visibility = type == MessageBoxType.Warning ? Visibility.Visible : Visibility.Collapsed;
            InfoIcon.Visibility = type == MessageBoxType.Info ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
