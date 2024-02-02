using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Bing.Extensions;
using Bing.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartUnzip.Core;
using SmartUnzip.Core.Enums;
using SmartUnzip.Core.Models;

namespace SmartUnzip.Avalonia.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    public IEnumerable<UnzipPackageAfterHandleType> UnzipPackageAfterHandleTypes
        => Enum.GetValues(typeof(UnzipPackageAfterHandleType)).Cast<UnzipPackageAfterHandleType>();

    public IEnumerable<DuplicateFileHandleType> DuplicateFileHandleTypes
        => Enum.GetValues(typeof(DuplicateFileHandleType)).Cast<DuplicateFileHandleType>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUnzipPackageMovePath))]
    UnzipOptions _options;

    [ObservableProperty]
    private ObservableCollection<ArchiveFileInfo> _archiveFileInfos = [];

    private readonly IUnzipExtractor _unzipExtractor;

    public bool CanUnzip => ArchiveFileInfos.Any(x => (int) x.ExtractProgress != 1);

    public bool ShowUnzipPackageMovePath =>
        Options.UnzipPackageAfterHandleType == UnzipPackageAfterHandleType.MoveToFolder;

    public HomeViewModel()
    {
        Options = App.ServiceProvider.GetRequiredService<IOptions<UnzipOptions>>().Value!;

        _unzipExtractor = App.ServiceProvider.GetRequiredService<IUnzipExtractor>();

        _archiveFileInfos.CollectionChanged += (sender, args) => { OnPropertyChanged(nameof(CanUnzip)); };
    }

    [RelayCommand]
    public async Task Reset()
    {
        var originPath = @"C:\Users\zyknow\Desktop\temp\TestZips";
        var newPath = @"C:\Users\zyknow\Desktop\temp\TestZips - 副本";


        DirectoryHelper.Delete(newPath);
        DirectoryHelper.Copy(originPath, newPath);
        ArchiveFileInfos.Clear();
        await OnDrop([newPath]);
    }

    public async Task OnDrop(IEnumerable<string> filePaths)
    {
        var unzipExtractor = App.ServiceProvider.GetRequiredService<IUnzipExtractor>();


        filePaths = filePaths.ToList();

        if (!filePaths.IsEmpty())
        {
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
                var archiveFileInfos = await unzipExtractor.FindArchiveAsync(files, Options);
                ArchiveFileInfos.AddIfNotContains(archiveFileInfos);
            }

            if (!directories.IsEmpty())
            {
                var archiveFileInfos = await unzipExtractor.FindArchiveAsync(directories, Options);
                ArchiveFileInfos.AddIfNotContains(archiveFileInfos);
            }
        }
    }


    [RelayCommand]
    public async Task Unzip()
    {
        await _unzipExtractor.ExtractsAsync(ArchiveFileInfos.ToList(), Options);
    }

    public void StateChanged()
    {
        OnPropertyChanged(nameof(ShowUnzipPackageMovePath));
    }
}