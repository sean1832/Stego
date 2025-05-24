using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.Controls;

public sealed partial class SteganographyLsbControl : UserControl
{
    private Border[] _cells;
    public SteganographyLsbControl()
    {
        InitializeComponent();

        // Ordered so that index 0 → “1 bit”, index 1 → “2 bits”, … index 7 → “8 bits”
        _cells =
        [
            CellOne, CellTwo, CellThree, CellFour,
            CellFive, CellSix, CellSeven, CellEight
        ];

        LsbCountSlider.ValueChanged += LsbCountSlider_ValueChanged;
        UpdateLsbCells((int)LsbCountSlider.Value);
    }

    public static readonly DependencyProperty LsbCountProperty =
        DependencyProperty.Register("LsbCount", typeof(int), typeof(SteganographyLsbControl),
            new PropertyMetadata(1, OnLsbCountChanged));

    public int LsbCount
    {
        get => (int)GetValue(LsbCountProperty);
        set => SetValue(LsbCountProperty, value);
    }

    private static void OnLsbCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SteganographyLsbControl)d;
        control.UpdateLsbCells((int)e.NewValue);
    }

    private void LsbCountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateLsbCells((int)Math.Round(e.NewValue));
        LsbCount = (int)Math.Round(e.NewValue);
    }

    private void UpdateLsbCells(int bit)
    {
        // Brushes from your resources
        Brush onBrush = (Brush)Resources["SystemControlHighlightAccentBrush"];
        Brush offBrush = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i].Background = (i < bit)
                ? onBrush
                : offBrush;
        }
    }
}
