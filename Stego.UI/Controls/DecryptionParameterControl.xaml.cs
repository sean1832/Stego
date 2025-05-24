using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Stego.UI.ViewModel;
using System;

namespace Stego.UI.Controls
{
    public sealed partial class DecryptionParameterControl : UserControl
    {
        private DecryptionPageViewModel? _vm;
        public DecryptionParameterControl()
        {
            InitializeComponent();
            this.DataContextChanged += DecryptionParameterControl_DataContextChanged;
        }

        private void DecryptionParameterControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _vm = args.NewValue as DecryptionPageViewModel;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
