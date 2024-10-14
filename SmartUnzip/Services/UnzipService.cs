using System.Threading;
using SmartUnzip.Core;

namespace SmartUnzip.Services;

public class UnzipService
{
    private readonly ArchiveService _archiveService = new();
    private UnzipOptions _options;


    private string[] _excludeExtensions;

    public UnzipService(UnzipOptions options)
    {
        SetOptions(options);
    }

    public void SetOptions(UnzipOptions options)
    {
        _options = options;
        _excludeExtensions = _options.ExcludeSearchFileExtensions.Split(',');
    }


    List<string?> Passwords =>
    [
        "", .._options.UnzipSortByPasswordUseCount
            ? App.Settings.Passwords.OrderByDescending(x => x.UseCount).Select(x => x.Value).ToList()
            : App.Settings.Passwords.Select(x => x.Value).ToList()
    ];

    public async Task<string> TestArchiveFilePasswordAsync(string archiveFilePath,
        string? firstPassword = null,
        CancellationToken cancellationToken = default)
    {
        string? resPassword = null;
        var cancellationTokenSource = new CancellationTokenSource();
        var linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);
        var token = linkedTokenSource.Token;

        var passwords = Passwords.ToList();
        
        if(firstPassword != null)
        {
            passwords.Remove(firstPassword);
            passwords.Insert(0, firstPassword);
        }
        
        var tasks = passwords.Select(async password =>
        {
            await App.PasswordTestSemaphore.WaitAsync(token);
            if (cancellationTokenSource.IsCancellationRequested)
            {
                App.PasswordTestSemaphore.Release();
                return;
            }

            try
            {
                if (await _archiveService.TestArchivePasswordAsync(archiveFilePath, password, token))
                {
                    resPassword = password;
                    await cancellationTokenSource.CancelAsync(); // 直接取消，不用等待异步取消
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
        catch (OperationCanceledException e)
        {
    
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
    /// <param name="cancellationToken"></param>
    /// <returns>children archive file Paths</returns>
    public async Task<string> UnzipAsync(string archiveFilePath, string? password = null,
        CancellationToken cancellationToken = default)
    {
        await App.UnzipSemaphore.WaitAsync(cancellationToken); // 等待信号量许可

        try
        {
            string? outputPath = _options.OutputPath;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.GetDirectoryName(archiveFilePath);
            }

            outputPath = Path.Combine(outputPath!, Path.GetFileNameWithoutExtension(archiveFilePath));

            if (File.Exists(outputPath))
            {
                outputPath = @$"{outputPath} 1";
                App.UnzipSemaphore.Release();
                await _archiveService.ExtractAsync(archiveFilePath, outputPath, password,
                    _options.DuplicateFileHandleMode, cancellationToken);

                return outputPath;
            }


            await _archiveService.ExtractAsync(archiveFilePath, outputPath, password,
                _options.DuplicateFileHandleMode, cancellationToken);

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
    public async Task<List<ArchiveFileValidResult>> SearchArchiveFileAsync(string dirPath,
        CancellationToken cancellationToken = default)
    {
        var archiveFiles = await _archiveService.SearchArchiveFilesAsync(dirPath, cancellationToken);

        return archiveFiles;
    }


    public async Task<ArchiveFileValidResult> ValidFileAsync(string path,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(path).TrimStart('.');

        if (_excludeExtensions
            .Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return new();
        }
        else
        {
            return await _archiveService.TestArchiveAsync(path, cancellationToken);
        }
    }
}