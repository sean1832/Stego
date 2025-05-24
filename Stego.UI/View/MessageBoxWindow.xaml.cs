using Microsoft.UI;
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
using WinRT.Interop;

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

        // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


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
            // Set this modal window's owner (the main application window).
            // The main window can be retrieved from App.xaml.cs if it's set as a static property.
            SetWindowOwner(owner: App.MainWindow);

            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;
            presenter.IsAlwaysOnTop = true;
            presenter.IsModal = true;

            AppWindow.SetPresenter(presenter);
            // After content is loaded, position at cursor
            CenterWindowAtScreen();

            // Show modal window
            AppWindow.Show();
        }

        // Sets the owner window of the modal window.
        private void SetWindowOwner(Window owner)
        {
            // Get the HWND (window handle) of the owner window (main window).
            IntPtr ownerHwnd = WindowNative.GetWindowHandle(owner);

            // Get the HWND of the AppWindow (modal window).
            IntPtr ownedHwnd = Win32Interop.GetWindowFromWindowId(AppWindow.Id);

            // Set the owner window using SetWindowLongPtr for 64-bit systems
            // or SetWindowLong for 32-bit systems.
            if (IntPtr.Size == 8) // Check if the system is 64-bit
            {
                SetWindowLongPtr(ownedHwnd, -8, ownerHwnd); // -8 = GWLP_HWNDPARENT
            }
            else // 32-bit system
            {
                SetWindowLong(ownedHwnd, -8, ownerHwnd); // -8 = GWL_HWNDPARENT
            }
        }


        private void CenterWindowAtScreen()
        {
            var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
            if (area == null) return;
            AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
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
