using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using WinRT.Interop;
using Stego.UI.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stego.UI.Controls;

public sealed partial class ImageSelectorControl : UserControl
{
    public ImageSelectorControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty SelectedFilePathProperty =
        DependencyProperty.Register(
            nameof(SelectedFilePath),
            typeof(string),
            typeof(ImageSelectorControl),
            new PropertyMetadata(null, OnSelectedFilePathChanged)
        );

    public string? SelectedFilePath
    {
        get => (string?)GetValue(SelectedFilePathProperty);
        set => SetValue(SelectedFilePathProperty, value);
    }

    private static void OnSelectedFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // react to external changes, clear image if someone sets it back to null
        var control = (ImageSelectorControl)d;
        if (e.NewValue == null)
        {
            control.CoverImage.Source = null;
            control.CoverImage.Visibility = Visibility.Collapsed;
            control.CoverImageSelectionButton.Visibility = Visibility.Visible;
            control.CoverImageActionPanel.Visibility = Visibility.Collapsed;
        }
    }

    private async void OnSelectImageClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();

        var window = App.MainWindow;
        IntPtr hwnd = WindowNative.GetWindowHandle(window);
        // hook up the file picker to the current window
        InitializeWithWindow.Initialize(picker, hwnd);

        // only lossless
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return; // User cancelled the file picker

        // set the dependency property so parent can react to it
        SelectedFilePath = file.Path;

        using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            CoverImage.Source = bitmapImage;
        }

        // set visibility
        CoverImage.Visibility = Visibility.Visible;
        CoverImageSelectionButton.Visibility = Visibility.Collapsed;
        CoverImageActionPanel.Visibility = Visibility.Visible;
    }

    private void OnRemoveImageClicked(object sender, RoutedEventArgs e)
    {
        CoverImage.Source = null;
        CoverImage.Visibility = Visibility.Collapsed;
        CoverImageSelectionButton.Visibility = Visibility.Visible;
        CoverImageActionPanel.Visibility = Visibility.Collapsed;

        // clear the dependency property
        SelectedFilePath = null;
    }
}
