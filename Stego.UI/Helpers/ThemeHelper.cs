using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stego.UI.Helpers
{
    internal class ThemeHelper
    {
        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                Window window = App.MainWindow;
                if (window.Content is FrameworkElement rootElement)
                {
                    if (rootElement.RequestedTheme != ElementTheme.Default)
                    {
                        return rootElement.RequestedTheme;
                    }
                }

                return GetEnum<ElementTheme>(App.Current.RequestedTheme.ToString());
            }
        }


        /// <summary>
        /// Converts a string into an enum.
        /// </summary>
        /// <typeparam name="TEnum">The output enum type.</typeparam>
        /// <param name="text">The input text.</param>
        /// <returns>The parsed enum.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the TEnum type is not a enum.</exception>
        private static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }
    }
}
