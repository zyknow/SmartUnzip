using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Bing.Collections;
using Bing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;
using SmartUnzip.Core;

namespace SmartUnzip.Avalonia.Views;

public partial class HomeView : UserControl
{
    public HomeViewModel vm { get; set; }

    public HomeView()
    {
        InitializeComponent();
        vm = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        DataContext = vm;
        // unzipPackageAfterHandleTypeComBox.SelectedItem = UnzipPackageAfterHandleTypeEnumNames[0];
        GridDragDrop.AddHandler(DragDrop.DropEvent, Grid_Drop);
        DragDrop.SetAllowDrop(this, true);
    }

    private async Task Grid_Drop(object sender, DragEventArgs e)
    {
        var unzipExtractor = App.ServiceProvider.GetRequiredService<IUnzipExtractor>();


        if (e.Data.Contains(DataFormats.Files))
        {
            var filePaths = e.Data.GetFileNames();

            List<FileInfo> files = [];
            List<DirectoryInfo> directories = [];

            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                    files.Add(new FileInfo(path));
                else if (Directory.Exists(path))
                    directories.Add(new DirectoryInfo(path));
            }

            if (!files.IsEmpty())
            {
                var archiveFileInfos = await unzipExtractor.FindArchiveAsync(files, vm.Options);
                vm.ArchiveFileInfos.AddIfNotContains(archiveFileInfos);
            }
            
            if (!directories.IsEmpty())
            {
                var archiveFileInfos = await unzipExtractor.FindArchiveAsync(directories, vm.Options);
                vm.ArchiveFileInfos.AddIfNotContains(archiveFileInfos);
            }
        }
    }


    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        vm.StateChanged();
    }
}