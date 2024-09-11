using Avalonia.Platform.Storage;

namespace SmartUnzip.Helpers;

public class FileSystemPickerHelper
{
    public static async Task<IReadOnlyList<IStorageFolder>?> SelectFolderAsync(FolderPickerOpenOptions options)
    {
        var storageProvider = App.MainWindow.StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var folderResult = await storageProvider.OpenFolderPickerAsync(options);

            if (folderResult.Count > 0)
            {
                return folderResult;
            }
            else
            {
                return null;
            }
        }

        throw new Exception("不可选择文件夹");
    }

    public static async Task<IReadOnlyList<IStorageFile>?> SelectFileAsync(FilePickerOpenOptions options)
    {
        var storageProvider = App.MainWindow.StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var fileResult = await storageProvider.OpenFilePickerAsync(options);

            if (fileResult.Count > 0)
            {
                return fileResult;
            }
            else
            {
                return null;
            }
        }

        throw new Exception("不可选择文件");
    }
}