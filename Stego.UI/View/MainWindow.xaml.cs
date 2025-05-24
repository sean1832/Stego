using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Xaml.Media.Animation;
using Stego.UI.View;
using WinRT.Interop;
using AppWindow = Microsoft.UI.Windowing.AppWindow;
using Size = System.Drawing.Size;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public const double CollapseThreshold = 1000;
        public MainWindow()
        {
            InitializeComponent();
            InitializeWindowSize(new Size(1100, 800), new Size(800, 600));
            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

            SizeChanged += OnWindowSizeChanged;
            ContentFrame.Navigate(typeof(EncryptionPage));
            EncryptionNavigation.IsSelected = true;
        }

        private void InitializeWindowSize(Size windowSize, Size minWindowSize)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWindowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(myWindowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(windowSize.Width, windowSize.Height));
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = minWindowSize.Width;
                presenter.PreferredMinimumHeight = minWindowSize.Height;
            }
        }

        private void NavigationPanel_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer is NavigationViewItem navItem
                && navItem.Tag is string tag)
            {
                switch (tag)
                {
                    case "Encryption":
                        ContentFrame.Navigate(typeof(EncryptionPage));
                        EncryptionPage? encryptionPage = ContentFrame.Content as EncryptionPage;
                        if (encryptionPage != null)
                        {
                            encryptionPage.UpdateSplitView(Bounds.Width);
                        }
                        break;
                    case "Decryption":
                        ContentFrame.Navigate(typeof(DecryptionPage));
                        DecryptionPage? decryptionPage = ContentFrame.Content as DecryptionPage;
                        if (decryptionPage != null)
                        {
                            decryptionPage.UpdateSplitView(Bounds.Width);
                        }
                        break;
                }
            }
        }

        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateNavView(e.Size.Width);
            
            if (ContentFrame.Content is EncryptionPage encryptionPage)
            {
                encryptionPage.UpdateSplitView(e.Size.Width);
            }
            else if (ContentFrame.Content is DecryptionPage decryptionPage)
            {
                decryptionPage.UpdateSplitView(e.Size.Width);
            }
        }

        private void UpdateNavView(double width)
        {
            if (width <= CollapseThreshold)
            {
                NavigationPanel.IsPaneOpen = false;
            }
            else
            {
                NavigationPanel.IsPaneOpen = true;
            }
        }
    }
}
