using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SmartUnzip.Core;
using SmartUnzip.Helpers;

namespace SmartUnzip.Components;

public partial class UnzipOptionsConfigContent : UserControl
{
    public UnzipOptionsConfigContent()
    {
        InitializeComponent();
        SelectOutputFolderCommand = new RelayCommand(ShowSelectOutputFolder);
    }

    public ICommand SelectOutputFolderCommand { get; }


    // UnzipOptions
    public static readonly StyledProperty<UnzipOptions> UnzipOptionsProperty =
        AvaloniaProperty.Register<UnzipOptionsConfigContent, UnzipOptions>(nameof(UnzipOptions));

    public UnzipOptions UnzipOptions
    {
        get => GetValue(UnzipOptionsProperty);
        set => SetValue(UnzipOptionsProperty, value);
    }

    async void ShowSelectOutputFolder()
    {
        var paths = await FileSystemPickerHelper.SelectFolderAsync(new FolderPickerOpenOptions
        {
            Title = "请选择文件夹",
            AllowMultiple = false
        });

        if (paths == null || !paths.Any())
            return;

        UnzipOptions.OutputPath = paths.First().Path.LocalPath;
    }
}