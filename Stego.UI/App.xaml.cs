using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Stego.UI.Helpers;
using Stego.UI.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow MainWindow = new();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            // UI-thread exceptions
            UnhandledException += OnUIThreadException;

            // CLR exceptions on any thread
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            // Faulted Tasks that go unobserved
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnUIThreadException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowError($"UI Exception: {e.Exception.Message}");
        }

        private void OnDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            ShowError($"Domain Exception: {ex?.Message}");
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            ShowError($"Unobserved Task Exception: {e.Exception.Message}");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                MainWindow.Activate();
            }
            catch (Exception ex)
            {
                ShowError($"Launch Exception: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            // dispatch back to UI thread if necessary
            MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                MessageBox.Error(message, "Fatal Error");
            });
        }
    }
}
