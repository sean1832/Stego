using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.Controls
{
    public sealed partial class Argon2ParameterControl : UserControl
    {
        public Argon2ParameterControl()
        {
            InitializeComponent();
        }

        private void Argon2CostSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Argon2CostSlider.Header = $"Cost {Argon2CostSlider.Value}";
        }

        private void Argon2MemorySlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Argon2MemorySlider.Header = $"Memory(KB) {Argon2MemorySlider.Value}";
        }
    }
}
