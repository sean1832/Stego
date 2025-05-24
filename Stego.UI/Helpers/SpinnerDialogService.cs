using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stego.UI.Helpers
{
    public static class SpinnerDialogService
    {
        /// <summary>
        /// Shows a non-dismissable "working…" dialog with a ProgressRing,
        /// runs your async work, then closes the dialog and returns the result.
        /// </summary>
        public static async Task<T> ShowWhileAsync<T>(
            FrameworkElement host,
            string title,
            Func<Task<T>> work)
        {
            // build spinner
            var ring = new ProgressRing
            {
                IsActive = true,
                Width = 60,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var dlg = new ContentDialog
            {
                XamlRoot = host.XamlRoot,
                Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                Title = title,
                Content = ring,
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = false
            };

            // fire-and-forget showing it
            _ = dlg.ShowAsync();

            // do the real work
            T result = await work();

            // then hide
            dlg.Hide();

            return result;
        }

        /// <summary>
        /// Overload for work that doesn't return a value.
        /// </summary>
        public static async Task ShowWhileAsync(
            FrameworkElement host,
            string title,
            Func<Task> work)
        {
            await ShowWhileAsync<object>(host, title, async () => {
                await work();
                return null;
            });
        }
    }
}
