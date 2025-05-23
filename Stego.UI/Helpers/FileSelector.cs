using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;

namespace Stego.UI.Helpers
{

    public class FileSelectorSaveOptions
    {
        public PickerLocationId StartLocation { get; set; } = PickerLocationId.DocumentsLibrary;
        public Dictionary<string, IList<string>> FileTypeChoices { get; set; }
            = new Dictionary<string, IList<string>>();
        public string SuggestedFileName { get; set; } = "";
    }

    public static class FileSelector
    {
        public static async Task<StorageFile?> PickSaveFileAsync(FileSelectorSaveOptions opts)
        {
            FileSavePicker savePicker = new FileSavePicker();
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = opts.StartLocation;
            foreach (var kv in opts.FileTypeChoices)
                savePicker.FileTypeChoices.Add(kv.Key, kv.Value);
            savePicker.SuggestedFileName = opts.SuggestedFileName;

            return await savePicker.PickSaveFileAsync();
        }

        public static async Task<(bool, StorageFile?)> SaveAsync(FileSelectorSaveOptions opts, byte[] content)
        {
            StorageFile? file = await PickSaveFileAsync(opts);
            if (file == null) return (false, null);    // user cancelled

            CachedFileManager.DeferUpdates(file);
            await FileIO.WriteBytesAsync(file, content);
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

            return (status == FileUpdateStatus.Complete
                    || status == FileUpdateStatus.CompleteAndRenamed, file);
        }
    }
}
