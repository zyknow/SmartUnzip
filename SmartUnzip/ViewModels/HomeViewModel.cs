using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SmartUnzip.Core;

namespace SmartUnzip.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        AddUnzipItem();
    }

    public ObservableCollection<UnzipItemViewModel> Unzips { get; set; } =
    [
        // new UnzipItemViewModel(App.Settings.UnzipOptions)
        // {
        //     Items =
        //     [
        //         new UnzipTreeItemViewModel(@$"G:\Avalonia Temps\AtomUI\test.zip")
        //         {
        //             ExtractStatus = ExtractStatus.Testing,
        //             IsVolume = true,
        //             Children = [new UnzipTreeItemViewModel(@$"G:\Avalonia Temps\AtomUI\test1.zip")]
        //         }
        //     ]
        // }
    ];

    [RelayCommand]
    void RemoveUnzipItem(UnzipItemViewModel item)
    {
        Unzips.Remove(item);
    }

    [RelayCommand]
    void AddUnzipItem()
    {
        // 深拷贝
        Unzips.Add(new UnzipItemViewModel());
    }
}