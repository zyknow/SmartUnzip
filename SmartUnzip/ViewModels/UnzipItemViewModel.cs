using System.Text.Json;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SmartUnzip.Core;
using SmartUnzip.Services;

namespace SmartUnzip.ViewModels;

public partial class UnzipItemViewModel : ViewModelBase
{
    [ObservableProperty] private UnzipOptions _unzipOptions;

    [ObservableProperty] private bool _showAdvanced;

    [ObservableProperty] private bool _isRunning;

    [NotifyPropertyChangedFor(nameof(IsFinish))] [ObservableProperty]
    int _progress;


    public bool IsFinish => Progress == 100;

    public IBrush ProgressColor => IsFinish ? Brushes.LawnGreen : Brushes.LightBlue;

    public ObservableCollection<UnzipTreeItemViewModel> Items { get; set; } = [];

    CancellationTokenSource _cancellationTokenSource;

    private readonly UnzipService _unzipService;

    public UnzipItemViewModel()
    {
        UnzipOptions = new UnzipOptions();
        UnzipOptions = JsonSerializer.Deserialize<UnzipOptions>(JsonSerializer.Serialize(App.Settings.UnzipOptions))!;
    }

    [RelayCommand]
    async Task StopUnzipAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        IsRunning = false;
    }

    [RelayCommand]
    async Task StartUnzipAsync()
    {
        if (string.IsNullOrWhiteSpace(App.Settings.SevenZFilePath) || !File.Exists(App.Settings.SevenZFilePath))
        {
            await MessageBox.ShowOverlayAsync("请设置正确的7z.exe路径");
            return;
        }

        using (_cancellationTokenSource = new CancellationTokenSource())
        {
            IsRunning = true;
            _unzipService.SetOptions(UnzipOptions);
            try
            {
                await ItemUnzipAsync(Items);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            IsRunning = false;
        }
    }

    // 递归操作Items
    public async Task ItemUnzipAsync(IEnumerable<UnzipTreeItemViewModel> items)
    {
        if (!items.Any())
            return;

        foreach (var item in items)
        {
            await ItemUnzipAsync(item);
        }
    }

    public async Task ItemUnzipAsync(UnzipTreeItemViewModel item)
    {
        try
        {
            await ArchiveErrorHandleAsync(item);

            await TestPasswordHandleAsync(item);

            await ArchiveExtractHandleAsync(item);

            await ArchiveExtractHandleAsync(item);

            await ArchiveExtractSucceededHandleAsync(item);
        }
        catch (Exception e)
        {
            Log.Error(e, "ItemUnzipAsync Exception");
        }
    }


    public async Task ArchiveErrorHandleAsync(UnzipTreeItemViewModel item)
    {
        if (item.ExtractStatus == ExtractStatus.Error)
            item.ExtractStatus = ExtractStatus.None;
    }

    public async Task TestPasswordHandleAsync(UnzipTreeItemViewModel item)
    {
        if (item.ExtractStatus != ExtractStatus.None)
            return;

        item.ExtractStatus = ExtractStatus.Testing;
        try
        {
            var password =
                await _unzipService.TestArchiveFilePasswordAsync(item.FilePath, _cancellationTokenSource.Token);
            item.Passowrd = password;
            var passwordModel =
                App.Settings.Passwords.FirstOrDefault(x => x.Value == password);
            if (passwordModel != null)
                passwordModel.UseCount += 1;
            item.ExtractStatus = ExtractStatus.Tested;
        }
        catch (Exception e)
        {
            Log.Error(e, "TestPasswordHandleAsync Exception");
            item.Message = e.Message;
            item.ExtractStatus = ExtractStatus.Error;
        }
    }

    public async Task ArchiveExtractHandleAsync(UnzipTreeItemViewModel item)
    {
        if (item.ExtractStatus != ExtractStatus.Tested)
            return;

        item.ExtractStatus = ExtractStatus.Extracting;

        try
        {
            item.OutputPath = await _unzipService.UnzipAsync(item.FilePath, item.Passowrd);

            item.ExtractStatus = ExtractStatus.ExtractSucceeded;
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                item.Message = @$"提取失败：{e}";
                item.ExtractStatus = ExtractStatus.Error;
            });
        }
    }

    public async Task ArchiveExtractSucceededHandleAsync(UnzipTreeItemViewModel item)
    {
        if (item.ExtractStatus != ExtractStatus.ExtractSucceeded)
            return;

        if (UnzipOptions.RecursiveUnzip)
        {
            await foreach (var archiveRes in _unzipService.SearchArchiveFileAsync(item.OutputPath))
            {
                item.Children.Add(new UnzipTreeItemViewModel(archiveRes.FilePath)
                {
                    IsVolume = archiveRes.IsVolume
                });
            }
        }

        if (UnzipOptions.UnzippedDeleteArchiveFile)
            File.Delete(item.FilePath);

        if (item.Children.Any())
            await ItemUnzipAsync(item.Children);

        var successCount = Items.Count(x => x.ExtractStatus == ExtractStatus.ExtractSucceeded);
        Progress = (int)((successCount / (double)Items.Count) * 100);
    }
}