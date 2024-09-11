using System.Threading;
using SmartUnzip.Core;

namespace SmartUnzip.Services;

public class UnzipService
{
    private readonly ArchiveService _archiveService = new();
    private UnzipOptions _options;


    private string[] _excludeExtensions;
    private string[] _includeExtensions;

    public UnzipService(UnzipOptions options)
    {
        SetOptions(options);
    }

    public void SetOptions(UnzipOptions options)
    {
        _options = options;
        _excludeExtensions = _options.ExcludeExtensions.Split(',');
        _includeExtensions = _options.IncludeExtensions.Split(',');
    }


    IReadOnlyList<string?> Passwords =>
    [
        "", .._options.UnzipSortByPasswordUseCount
            ? App.Settings.Passwords.OrderByDescending(x => x.UseCount).Select(x => x.Value).ToList()
            : App.Settings.Passwords.Select(x => x.Value).ToList()
    ];

    public async Task<string> TestArchiveFilePasswordAsync(string archiveFilePath,
        CancellationToken cancellationToken = default)
    {
        string? resPassword = null;
        var cancellationTokenSource = new CancellationTokenSource();
        var linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);
        var token = linkedTokenSource.Token;


        var tasks = Passwords.Select(async password =>
        {
            await App.PasswordTestSemaphore.WaitAsync(token);
            try
            {
                if (await _archiveService.TestArchivePasswordAsync(archiveFilePath, password, token))
                {
                    resPassword = password;
                    cancellationTokenSource.Cancel(); // 直接取消，不用等待异步取消
                }
            }
            catch (OperationCanceledException)
            {
                // 取消时正常退出，不处理
            }
            finally
            {
                App.PasswordTestSemaphore.Release();
            }
        }).ToArray();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // 捕获任务取消异常
        }

        if (resPassword == null)
        {
            throw new Exception("未找到密码");
        }

        return resPassword;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="archiveFilePath"></param>
    /// <param name="password"></param>
    /// <returns>children archive file Paths</returns>
    public async Task<string> UnzipAsync(string archiveFilePath, string? password = null)
    {
        await App.UnzipSemaphore.WaitAsync(); // 等待信号量许可

        try
        {
            string? outputPath = _options.OutputPath;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.GetDirectoryName(archiveFilePath);
            }

            outputPath = Path.Combine(outputPath!, Path.GetFileNameWithoutExtension(archiveFilePath));

            await _archiveService.ExtractAsync(archiveFilePath, outputPath, password,
                false,
                _options.DuplicateFileHandleMode);

            return outputPath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            App.UnzipSemaphore.Release();
        }
    }


    // 递归搜索文件
    public async IAsyncEnumerable<ArchiveFileSearchResult> SearchArchiveFileAsync(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            yield break;
        }

        var files = Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var res = await ValidFileAsync(file);

            if (res.IsValid)
            {
                yield return res;
            }
        }
    }


    public async Task<ArchiveFileValidResult> ValidFileAsync(string path)
    {
        var extension = Path.GetExtension(path).TrimStart('.');
        if (string.IsNullOrEmpty(extension))
        {
            return new();
        }

        if (_includeExtensions
            .Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return await _archiveService.TestArchiveAsync(path);
        }

        if (_excludeExtensions
            .Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return new();
        }


        return await _archiveService.TestArchiveAsync(path);
    }
}