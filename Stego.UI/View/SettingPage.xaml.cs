using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stego.UI.Helpers;
using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public string Version => AppInfoHelper.Version;
        public string AppTitleName => AppInfoHelper.Name;
        public string WinAppSdkRuntimeDetails => AppInfoHelper.WinAppSdkRuntimeDetails;

        public SettingPage()
        {
            InitializeComponent();
            Loaded += OnSettingsPageLoaded;
        }


        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = ThemeHelper.ActualTheme;
            switch (currentTheme)
            {
                case ElementTheme.Light:
                    ThemeMode.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    ThemeMode.SelectedIndex = 1;
                    break;
                case ElementTheme.Default:
                    ThemeMode.SelectedIndex = 2;
                    break;
            }
        }

        private async void BugRequestCard_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/sean1832/Stego/issues"));
        }

        private async void RepoCard_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/sean1832/Stego"));
        }

        private async void DonationCard_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://buymeacoffee.com/zekezhang"));
        }

        private void ThemeMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = (ThemeMode.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (tag is null) return;


            var window = App.MainWindow;
            if (Enum.TryParse<ElementTheme>(tag, out var theme))
            {
                if (App.MainWindow.Content is FrameworkElement root)
                {
                    root.RequestedTheme = theme;
                }
            }

            ApplyCaptionButtonColors(window, theme);
        }

        private void ApplyCaptionButtonColors(Window window, ElementTheme theme)
        {
            if (window is null) return;

            // Turn your WinUI3 Window into an AppWindow
            var hWnd = WindowNative.GetWindowHandle(window);
            var winId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(winId);
            var tb = appWindow.TitleBar;

            // always transparent so your XAML or acrylic shows through
            tb.ButtonBackgroundColor = Colors.Transparent;
            tb.ButtonInactiveBackgroundColor = Colors.Transparent;

            // pick foreground & hover/pressed tints
            if (theme == ElementTheme.Dark)
            {
                tb.ButtonForegroundColor = Colors.White;
            }
            else if (theme == ElementTheme.Light)
            {
                tb.ButtonForegroundColor = Colors.Black;
            }
            else
            {
                var currentTheme = ThemeHelper.ActualTheme;
                if (currentTheme == ElementTheme.Dark)
                {
                    tb.ButtonForegroundColor = Colors.White;
                }
                else if (currentTheme == ElementTheme.Light)
                {
                    tb.ButtonForegroundColor = Colors.Black;
                }
            }
        }
    }
}
