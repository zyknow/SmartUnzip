using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using SmartUnzip.Core;
using SmartUnzip.Services;

namespace SmartUnzip.ViewModels;

public partial class UnzipItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private UnzipOptions _unzipOptions;

    [ObservableProperty]
    private bool _showAdvanced;

    [ObservableProperty]
    private bool _isRunning;

    [NotifyPropertyChangedFor(nameof(IsFinish))]
    [ObservableProperty]
    int _progress;

    [ObservableProperty]
    private bool _isSearchingArchive;

    [ObservableProperty]
    private int _stopWatchTimeSecond;

    public Stopwatch Stopwatch = new();


    public bool IsFinish => Progress == 100;

    public IBrush ProgressColor => IsFinish ? Brushes.LawnGreen : Brushes.Yellow;

    public ObservableCollection<UnzipTreeItemViewModel> Items { get; set; } = [];

    CancellationTokenSource _cancellationTokenSource;

    private readonly UnzipService _unzipService;
    private readonly ArchiveService _archiveService = new();

    private readonly List<string> _unzippedDeleteFileRegexs;
    private readonly List<string> _unzippedDeleteFolderRegexs;

    public UnzipItemViewModel()
    {
        UnzipOptions = JsonSerializer.Deserialize<UnzipOptions>(JsonSerializer.Serialize(App.Settings.UnzipOptions))!;
        _unzipService = new UnzipService(UnzipOptions);
        _unzippedDeleteFileRegexs = UnzipOptions.UnzippedDeleteFileRegexs.Split(',')
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        _unzippedDeleteFolderRegexs = UnzipOptions.UnzippedDeleteFolderRegexs.Split(',')
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
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


        try
        {
            await Task.Run(async () =>
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    IsRunning = true;

                    Stopwatch.Restart();

                    Task.Run(async () =>
                    {
                        while (!_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            StopWatchTimeSecond = (int) Stopwatch.Elapsed.TotalSeconds;
                            await Task.Delay(1000);
                        }
                    });

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
                    StopWatchTimeSecond = (int) Stopwatch.Elapsed.TotalSeconds;
                    Stopwatch.Stop();
                }
            });
        }
        finally
        {
        }
    }

    // 递归操作Items
    public async Task ItemUnzipAsync(IEnumerable<UnzipTreeItemViewModel> unzipItems)
    {
        var items = unzipItems.ToList();
        if (!items.Any())
            return;

        // foreach (var item in items)
        // {
        //     await ItemUnzipAsync(item);
        // }

        var tasks = items.Select(ItemUnzipAsync).ToArray();

        await Task.WhenAll(tasks);
    }

    public async Task ItemUnzipAsync(UnzipTreeItemViewModel item)
    {
        try
        {
            await ArchiveErrorHandleAsync(item);

            await TestPasswordHandleAsync(item);

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
        {
            item.ExtractStatus = ExtractStatus.None;
            item.Message = "";
        }
    }

    public async Task TestPasswordHandleAsync(UnzipTreeItemViewModel item)
    {
        if (item.ExtractStatus != ExtractStatus.None)
            return;

        item.ExtractStatus = ExtractStatus.Testing;
        try
        {
            var password =
                await _unzipService.TestArchiveFilePasswordAsync(item.FilePath, item.Parent?.Password,
                    cancellationToken: _cancellationTokenSource.Token);
            item.Password = password;
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
            item.OutputPath =
                await _unzipService.UnzipAsync(item.FilePath, item.Password, _cancellationTokenSource.Token);

            item.ExtractStatus = ExtractStatus.ExtractSucceeded;
        }
        catch (Exception e)
        {
            Log.Error(e, "ArchiveExtractHandleAsync Exception");
            Dispatcher.UIThread.Invoke(() =>
            {
                item.Message = @$"解压失败";
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
            foreach (var archiveRes in await _unzipService.SearchArchiveFileAsync(item.OutputPath,
                         _cancellationTokenSource.Token))
            {
                item.Children.Add(new UnzipTreeItemViewModel(archiveRes.FilePath)
                {
                    IsVolume = archiveRes.IsVolume,
                    Parent = item
                });
            }
        }

        if (UnzipOptions.UnzippedDeleteArchiveFile)
        {
            File.Delete(item.FilePath);
            if (item.IsVolume)
            {
                // 删除分卷文件
                var volumeFiles = Directory.EnumerateFiles(Path.GetDirectoryName(item.FilePath),
                        $"{Path.GetFileNameWithoutExtension(item.FilePath)}.*", SearchOption.TopDirectoryOnly)
                    .ToArray();

                // 未匹配到分卷，可能存在多个.
                if (!volumeFiles.Any())
                {
                    volumeFiles = Directory.EnumerateFiles(Path.GetDirectoryName(item.FilePath),
                            $"{Path.GetFileNameWithoutExtension(item.FilePath.Split('.')[0])}.*",
                            SearchOption.TopDirectoryOnly)
                        .ToArray();
                }

                foreach (var volumeFile in volumeFiles)
                {
                    File.Delete(volumeFile);
                }
            }
        }

        await DeleteMatchedFilesAndFoldersAsync(item.OutputPath, _unzippedDeleteFileRegexs,
            _unzippedDeleteFolderRegexs);


        if (UnzipOptions.SingleSameNameFolderMoveUp)
        {
            SingleSameNameFolderMoveUp(item.OutputPath);
        }

        if (item.Children.Any())
            await ItemUnzipAsync(item.Children);

        if (item.Children.Any(x => x.ExtractStatus != ExtractStatus.Finished))
            return;

        var handleSingleFileOrFolder = CheckSingleFileOrFolder(item.OutputPath);

        if (handleSingleFileOrFolder)
        {
            var currentPath = item.OutputPath;
            // 确定目标路径为上一级目录
            var targetPath = Directory.GetParent(item.OutputPath)!.FullName;

            // 检查目标路径是否已存在文件夹
            if (Directory.Exists(targetPath))
            {
                // 如果是文件夹，则合并文件夹内容
                MergeDirectories(currentPath, targetPath);
                Directory.Delete(currentPath, true); // 删除源文件夹
            }
            else if (File.Exists(targetPath))
            {
                // 如果目标路径已有同名文件，则为当前文件重命名
                var newFileName = GetUniqueFileName(targetPath);
                File.Move(currentPath, newFileName); // 重命名并移动文件
            }
            else
            {
                // 如果目标路径不存在，直接移动文件或文件夹
                if (Directory.Exists(currentPath))
                {
                    Directory.Move(currentPath, targetPath); // 移动文件夹
                }
                else
                {
                    File.Move(currentPath, targetPath); // 移动文件
                }
            }
        }


        item.ExtractStatus = ExtractStatus.Finished;
        var successCount = Items.Count(x => x.ExtractStatus == ExtractStatus.Finished);
        Progress = (int) ((successCount / (double) Items.Count) * 100);

        OnPropertyChanged(nameof(IsFinish));
        OnPropertyChanged(nameof(ProgressColor));
    }


    public bool CheckSingleFileOrFolder(string dirPath)
    {
        var dirInfo = new DirectoryInfo(dirPath);

        var dirs = dirInfo.GetDirectories();
        var files = dirInfo.GetFiles().Where(x => !_archiveService.TestArchiveAsync(x.FullName).Result.IsValid)
            .ToArray();

        return dirs.Any() ^ files.Any();
    }

    // 递归删除匹配的文件和文件夹
    public async Task DeleteMatchedFilesAndFoldersAsync(string path, List<string> fileRegexs, List<string> folderRegexs)
    {
        if (!Directory.Exists(path))
            return;

        var directories = Directory.EnumerateDirectories(path);
        var files = Directory.EnumerateFiles(path);

        var deleteDirs = directories
            .Where(
                dir =>
                    folderRegexs.Any(pattern => Regex.IsMatch(dir, pattern))
            )
            .ToList();

        var deleteFiles = files
            .Where(
                file =>
                    fileRegexs.Any(pattern => Regex.IsMatch(file, pattern))
            )
            .ToList();

        deleteDirs.ForEach(d =>
        {
            try
            {
                Directory.Delete(d, true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Delete Folder Exception");
            }
        });

        deleteFiles.ForEach(f =>
        {
            try
            {
                File.Delete(f);
            }
            catch (Exception e)
            {
                Log.Error(e, "Delete File Exception");
            }
        });

        foreach (var directory in directories)
        {
            await DeleteMatchedFilesAndFoldersAsync(directory, fileRegexs, folderRegexs);
        }
    }

    void SingleSameNameFolderMoveUp(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);

        if (dirInfo.GetFiles().Any())
            return;

        if (dirInfo.GetDirectories().Count() != 1)
            return;

        var childDirInfo = dirInfo.GetDirectories()[0];

        var pathDirName = Path.GetFileName(path);

        if (childDirInfo.Name != pathDirName)
            return;

        // 文件夹内容移动到上一层
        MergeDirectories(childDirInfo.FullName, Directory.GetParent(childDirInfo.FullName)!.FullName);

        Directory.Delete(childDirInfo.FullName, true); // 删除源文件夹

        SingleSameNameFolderMoveUp(path);
    }

    void MergeDirectories(string sourceDir, string destinationDir)
    {
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));

            // 如果目标文件存在，重命名源文件
            if (File.Exists(destFile))
            {
                var newFileName = GetUniqueFileName(destFile);
                File.Move(file, newFileName);
            }
            else
            {
                File.Move(file, destFile);
            }
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destinationDir, Path.GetFileName(dir));

            // 如果子文件夹不存在，直接移动
            if (!Directory.Exists(destSubDir))
            {
                Directory.Move(dir, destSubDir);
            }
            else
            {
                // 递归合并子文件夹
                MergeDirectories(dir, destSubDir);
            }
        }
    }

// 获取唯一文件名（避免重名）
    string GetUniqueFileName(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        int count = 1;
        string newFilePath;

        // 循环直到找到一个不重复的文件名
        do
        {
            newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension} ({count}){extension}");
            count++;
        } while (File.Exists(newFilePath));

        return newFilePath;
    }
}