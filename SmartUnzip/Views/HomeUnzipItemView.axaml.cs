using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SmartUnzip.Core;
using SmartUnzip.Services;
using SmartUnzip.ViewModels;

namespace SmartUnzip.Views;

public partial class HomeUnzipItemView : UserControl
{
    public HomeUnzipItemView()
    {
        InitializeComponent();

        DragBorder.AddHandler(DragDrop.DropEvent, Drop);
    }

    public async void Drop(object sender, DragEventArgs e)
    {
        var vm = DataContext as UnzipItemViewModel;
        var items = vm!.Items;
        var options = vm.UnzipOptions;
        var unzipService = new UnzipService(options);

        void AddFileIfNotContains(string filePath, bool isVolume)
        {
            if (items.Any(x => x.FilePath == filePath)) return;

            var item = new UnzipTreeItemViewModel(filePath);
            items.Add(item);
        }


        if (e.Data.Contains(DataFormats.Files))
        {
            var filePaths = e.Data.GetFiles();

            if (filePaths?.Any() != true)
                return;

            foreach (var filePath in filePaths)
            {
                var path = filePath.TryGetLocalPath();

                if (string.IsNullOrEmpty(path))
                    continue;

                var isDir = Directory.Exists(path);
                var isFile = !isDir && File.Exists(path);


                if (isDir)
                {
                    await foreach (var archiveRes in unzipService.SearchArchiveFileAsync(path))
                    {
                        AddFileIfNotContains(archiveRes.FilePath, archiveRes.IsVolume);
                    }
                }
                else if (isFile)
                {
                    var archiveRes = await unzipService.ValidFileAsync(path);
                    if (archiveRes.IsValid)
                    {
                        AddFileIfNotContains(archiveRes.FilePath, archiveRes.IsVolume);
                    }
                }
            }
        }
    }
}