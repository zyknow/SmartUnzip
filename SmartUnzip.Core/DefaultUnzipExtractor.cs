using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpCompress.Common;
using SmartUnzip.Core.Enums;

namespace SmartUnzip.Core;

public class DefaultUnzipExtractor(
    IPasswordRepository passwordRepository,
    ILogger<DefaultUnzipExtractor> logger,
    IOptions<UnzipOptions> unzipOptions,
    IArchiveProcessor archiveProcessor) : IUnzipExtractor, ISingletonDependency
{
    // public virtual async IAsyncEnumerable<string> ExtractsAsync(List<ArchiveFileInfo> archiveFileInfo)
    // {
    //     if (archiveFileInfo.IsNullOrEmpty())
    //         throw new ArgumentNullException(nameof(archiveFileInfo));
    //
    //     // 创建一个SemaphoreSlim，允许同时运行的任务数量为5
    //     var semaphore = new SemaphoreSlim(unzipOptions.Value.MaxUnzipArchiveCount);
    //
    //     // 使用Task.WhenAll来等待所有任务完成
    //     var tasks = archiveFileInfo.Select(async info =>
    //     {
    //         await semaphore.WaitAsync();
    //         try
    //         {
    //             await ExtractAsync(info);
    //             // 当处理完成后，返回 info.ExtractDirectory
    //             yield return info.ExtractDirectory;
    //         }
    //         finally
    //         {
    //             semaphore.Release();
    //         }
    //     });
    //
    //     // 等待所有任务完成并返回结果
    //     foreach (var task in tasks)
    //     {
    //         // 由于我们正在处理 IAsyncEnumerable，我们需要使用 await foreach
    //         await foreach (var directory in task)
    //         {
    //             yield return directory;
    //         }
    //     }
    // }

    public virtual async IAsyncEnumerable<ArchiveFileInfo> ExtractsAsync(List<ArchiveFileInfo> archiveFileInfo)
    {
        if (archiveFileInfo.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(archiveFileInfo));

        var maxUnzipArchiveCount = unzipOptions.Value.MaxUnzipArchiveCount;
        if (maxUnzipArchiveCount == 0)
            maxUnzipArchiveCount = 1;
        
        // 创建一个SemaphoreSlim，允许同时运行的任务数量
        var semaphore = new SemaphoreSlim(maxUnzipArchiveCount);

        // 为每个文件信息创建一个任务
        var tasks = archiveFileInfo.Select(info => ExtractAndReturnDirectoryAsync(info, semaphore));

        // 等待任务完成并返回结果
        foreach (var task in tasks)
        {
            // 使用 await foreach 来迭代异步结果
            await foreach (var directory in task)
            {
                yield return directory;
            }
        }
    }

    private async IAsyncEnumerable<ArchiveFileInfo> ExtractAndReturnDirectoryAsync(ArchiveFileInfo info,
        SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            await ExtractAsync(info);
            yield return info;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public virtual async Task ExtractAsync(ArchiveFileInfo archiveFileInfo)
    {
        if (archiveFileInfo is null)
            throw new ArgumentNullException(nameof(archiveFileInfo));

        // TODO: 完成DuplicateFileHandleType剩余的两个选项处理
        archiveProcessor.Extractor(archiveFileInfo,
            opt =>
            {
                opt.Overwrite = unzipOptions.Value.DuplicateFileHandleType == DuplicateFileHandleType.Overwrite;
            });
    }

    public virtual void TestedOpenArchive(ArchiveFileInfo archiveFileInfo)
    {
        var lostParts = archiveFileInfo.Parts.Where(x => !File.Exists(x)).ToList();

        if (lostParts.Any())
            throw new FileNotFoundException("未找到文件。", lostParts.First());

        var passwords = GetPasswords();

        var partList = archiveFileInfo.Parts.ToList();

        Exception? ex = null;


        foreach (var unzipPassword in passwords)
        {
            try
            {
                var archive = archiveProcessor.OpenArchive(partList, opt => { opt.Password = unzipPassword?.Value; });

                archiveFileInfo.Password = unzipPassword?.Value;
                archiveFileInfo.TestedPassword = true;
                archiveFileInfo.Archive = archive;

                if (unzipPassword == null)
                    return;

                unzipPassword.IncreaseUseCount();
                passwordRepository.UpdatePassword(unzipPassword);

                return;
            }
            catch (CryptographicException e)
            {
                logger.LogError(e, "密码错误。");
                ex = new UserFriendlyException(@$"测试 {unzipPassword?.Value} 不正确");
            }
            catch (Exception e)
            {
                ex = e;
                logger.LogError(e, "测试打开压缩文件异常。");
            }
        }

        archiveFileInfo.TestedPassword = true;
        archiveFileInfo.Exception = ex;
        throw ex;
    }

    List<UnzipPassword> GetPasswords()
    {
        List<UnzipPassword> passwords =
            [null, ..passwordRepository.GetAllPasswords().OrderBy(x => x.UseCount).ToList()];

        return passwords;
    }
}