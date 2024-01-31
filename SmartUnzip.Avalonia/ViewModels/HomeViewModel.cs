using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
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

    public bool CanUnzip => ArchiveFileInfos.Any(x=> (int)x.ExtractProgress != 1);

    public bool ShowUnzipPackageMovePath =>
        Options.UnzipPackageAfterHandleType == UnzipPackageAfterHandleType.MoveToFolder;

    public HomeViewModel()
    {
        Options = App.ServiceProvider.GetRequiredService<IOptions<UnzipOptions>>().Value!;
        _archiveFileInfos.CollectionChanged += (sender, args) => { OnPropertyChanged(nameof(CanUnzip)); };
    }

    public void FilesDataGrid_Drop(object sender, DragEventArgs e)
    {
    }

    public void StateChanged()
    {
        OnPropertyChanged(nameof(ShowUnzipPackageMovePath));
    }
}