using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
namespace Stego.UI.Controls
{
    public sealed partial class PasswordPromptContent : UserControl
    {
        public string Password => PasswordBox.Password;

        public PasswordPromptContent()
        {
            InitializeComponent();
        }

        public void ShowError(string msg)
        {
            ErrorTextBlock.Text = msg;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        public void ShowSpinner()
        {
            // disable input
            PasswordBox.IsEnabled = false;
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            // show spinner
            ProgressRing.IsActive = true;
            ProgressRing.Visibility = Visibility.Visible;
        }
    }
}
