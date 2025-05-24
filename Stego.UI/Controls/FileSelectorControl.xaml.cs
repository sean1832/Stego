using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;


namespace Stego.UI.Controls
{
    public sealed partial class FileSelectorControl : UserControl
    {
        public string FileTypes { get; set; } = string.Empty;

        public FileSelectorControl()
        {
            InitializeComponent();

            GeneralFile.Visibility = Visibility.Collapsed;
        }

        public static readonly DependencyProperty SelectedFilePathProperty =
            DependencyProperty.Register(
                nameof(SelectedFilePath),
                typeof(string),
                typeof(FileSelectorControl),
                new PropertyMetadata(null, OnSelectedFilePathChanged)
            );

        public string? SelectedFilePath
        {
            get => (string?)GetValue(SelectedFilePathProperty);
            set => SetValue(SelectedFilePathProperty, value);
        }
        public event EventHandler<string?>? SelectedFilePathChanged;

        private static void OnSelectedFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // react to external changes, clear image if someone sets it back to null
            var control = (FileSelectorControl)d;
            if (e.NewValue == null)
            {
                control.CoverImage.Source = null;
                control.CoverImage.Visibility = Visibility.Collapsed;
                control.GeneralFile.Visibility = Visibility.Collapsed;
                control.FileSelectionButton.Visibility = Visibility.Visible;
                control.FileActionPanel.Visibility = Visibility.Collapsed;
            }
            control.SelectedFilePathChanged?.Invoke(control, (string?)e.NewValue);
        }

        private async void OnSelectFileClicked(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            var window = App.MainWindow;
            IntPtr hwnd = WindowNative.GetWindowHandle(window);
            // hook up the file picker to the current window
            InitializeWithWindow.Initialize(picker, hwnd);

            if (!string.IsNullOrEmpty(FileTypes))
            {
                List<string> fileTypes = new(FileTypes.Split(','));
                foreach (var fileType in fileTypes)
                {
                    picker.FileTypeFilter.Add(fileType);
                }
            }
            else
            {
                picker.FileTypeFilter.Add("*");
            }

            StorageFile? file = await picker.PickSingleFileAsync();
            if (file == null)
                return; // User cancelled the file picker

            // set the dependency property so parent can react to it
            SelectedFilePath = file.Path;


            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                if (file.Path.EndsWith(".png") || file.Path.EndsWith(".bmp"))
                {
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    CoverImage.Source = bitmapImage;
                    CoverImage.Visibility = Visibility.Visible;
                    GeneralFile.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CoverImage.Visibility = Visibility.Collapsed;
                    GeneralFile.Visibility = Visibility.Visible;
                    GeneralFileName.Text = Path.GetFileName(file.Path);
                    Tuple<double, string> fileSizeData = ConvertFileSizeUnit(await GetFileSizeAsync(file));
                    GeneralFileSize.Text = $"{fileSizeData.Item1} {fileSizeData.Item2}";
                }
            }

            FileSelectionButton.Visibility = Visibility.Collapsed;
            FileActionPanel.Visibility = Visibility.Visible;
        }

        private async Task<ulong> GetFileSizeAsync(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            return properties.Size;
        }

        private Tuple<double, string> ConvertFileSizeUnit(ulong fileSize)
        {
            string[] sizeUnits = ["B", "KB", "MB", "GB", "TB"];
            double size = fileSize;
            int unitIndex = 0;
            while (size >= 1024 && unitIndex < sizeUnits.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return new Tuple<double, string>(Math.Round(size, 2), sizeUnits[unitIndex]);
        }

        private void OnRemoveFileClicked(object sender, RoutedEventArgs e)
        {
            CoverImage.Source = null;
            CoverImage.Visibility = Visibility.Collapsed;
            GeneralFile.Visibility = Visibility.Collapsed;
            FileSelectionButton.Visibility = Visibility.Visible;
            FileActionPanel.Visibility = Visibility.Collapsed;

            SelectedFilePath = null;
        }
    }
}
