using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SmartUnzip.Core;
using SmartUnzip.Helpers;

namespace SmartUnzip.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public SettingsOptions SettingsOptions => App.Settings;

    [RelayCommand]
    void SaveSettings()
    {
        SettingsOptions.SaveSettings();
    }
    
    [RelayCommand]
    async Task SelectSevenZFilePathAsync()
    {
        var paths = await FileSystemPickerHelper.SelectFileAsync(new FilePickerOpenOptions()
        {
            Title = "请选择7z.exe",
            AllowMultiple = false
        });

        if (paths == null || !paths.Any())
            return;

        SettingsOptions.SevenZFilePath = paths.First().Path.LocalPath;
    }
}