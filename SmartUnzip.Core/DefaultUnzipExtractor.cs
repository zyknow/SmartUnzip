using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SmartUnzip.Core.Enums;
using SmartUnzip.Core.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Volo.Abp.IO;

namespace SmartUnzip.Core;

public class DefaultUnzipExtractor(
    IPasswordRepository passwordRepository,
    ILogger<DefaultUnzipExtractor> logger, IServiceProvider serviceProvider) : IUnzipExtractor, ISingletonDependency
{

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task ExtractsAsync(List<ArchiveFileInfo> archiveFileInfo, UnzipOptions options)
    {
        if (archiveFileInfo.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(archiveFileInfo));

        var maxUnzipArchiveCount = options.MaxUnzipArchiveCount;
        if (maxUnzipArchiveCount == 0)
            maxUnzipArchiveCount = 1;

        // 创建一个SemaphoreSlim，允许同时运行的任务数量
        var semaphore = new SemaphoreSlim(maxUnzipArchiveCount);

        // 为每个文件信息创建一个任务
        var tasks = archiveFileInfo.Select(info => ExtractsAsync(info, semaphore, options));

        // 等待任务完成并返回结果
        foreach (var task in tasks)
        {

            // 使用 await 来等待任务完成
            await task;

            //var result = await task;

            //yield return result;

            //// 使用 await foreach 来迭代异步结果
            //await foreach (var directory in task)
            //{
            //    yield return directory;
            //}
        }
    }

    /// <summary>
    /// 获取压缩文件信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="UserFriendlyException"></exception>
    /// <returns></returns>
    public virtual ArchiveFileInfo GetArchiveFileInfo(string filePath, List<string>? excludeRegexs = null,
        List<ArchiveType>? supportArchiveTypes = null)
    {
        CheckMatchArchiveFile(filePath);

        var parts = ArchiveFactory.GetFileParts(filePath).ToList();

        if (parts.IsNullOrEmpty())
            throw new UserFriendlyException("未找到压缩文件的所有分卷或本体");

        var archiveFileInfo = new ArchiveFileInfo
        {
            FilePath = parts.FirstOrDefault(),
            Parts = parts
        };

        return archiveFileInfo;
    }

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public virtual Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(string directory, UnzipOptions options,
        bool recursive = true) => FindArchiveAsync(new DirectoryInfo(directory), options, recursive);

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(DirectoryInfo directory, UnzipOptions options,
        bool recursive = true)
    {
        List<ArchiveFileInfo> infos = [];

        var files = directory.GetFiles();

        if (files.IsNullOrEmpty())
            return [];

        foreach (var fileInfo in files)
        {
            // 排除已经处理过的文件
            if (infos.Any(info => info.Parts.Contains(fileInfo.FullName)))
                continue;

            try
            {
                var info = GetArchiveFileInfo(fileInfo.FullName, options.ExcludeRegexs,
                    options.SupportArchiveTypes);
                infos.Add(info);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        if (!recursive) return infos;

        var subDirectories = directory.GetDirectories();
        foreach (var subDirectory in subDirectories)
        {
            infos.AddRange(await FindArchiveAsync(subDirectory, options, true));
        }

        return infos;
    }

    protected virtual void OpenArchive(ArchiveFileInfo archiveFileInfo, UnzipOptions options)
    {
        var lostParts = archiveFileInfo.Parts.Where(x => !File.Exists(x)).ToList();

        if (lostParts.Any())
            throw new FileNotFoundException("未找到文件。", lostParts.First());

        var passwords = GetPasswords(options.ChoicePasswordOrderByUseCount);

        var partList = archiveFileInfo.Parts.ToList();

        Exception? ex = null;


        foreach (var unzipPassword in passwords)
        {
            try
            {
                var archive = OpenArchiveUsePassword(partList, unzipPassword?.Value);

                archiveFileInfo.Password = unzipPassword?.Value;
                archiveFileInfo.HasTestedPassword = true;
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

        archiveFileInfo.HasTestedPassword = true;
        archiveFileInfo.Exception = ex;
        throw ex;
    }


    protected virtual async Task ExtractsAsync(ArchiveFileInfo info, SemaphoreSlim semaphore,
        UnzipOptions options)
    {
        await semaphore.WaitAsync();

        try
        {

            if (info.Archive is null)
                OpenArchive(info, options);

            info.Children = new ObservableCollection<ArchiveFileInfo>(await ExtractAsync(info, options));

            foreach (ArchiveFileInfo childArchiveFileInfo in info.Children)
            {
                await ExtractsAsync(childArchiveFileInfo, semaphore, options);
            }

        }
        catch (Exception e)
        {
            info.Exception = e;
            logger.LogError(e, e.Message);
        }
        finally
        {
            info.Archive?.Dispose();
            semaphore.Release();
        }
    }

    protected virtual async Task<IEnumerable<ArchiveFileInfo>> ExtractAsync(ArchiveFileInfo archiveFileInfo, UnzipOptions options)
    {
        if (archiveFileInfo is null)
            throw new ArgumentNullException(nameof(archiveFileInfo));

        var archive = archiveFileInfo.Archive;

        if (archive is null)
        {
            OpenArchive(archiveFileInfo, options);
            //throw new UserFriendlyException("未打开压缩文件");
        }

        if (!options.UnzipDirectory.IsNullOrEmpty() && archiveFileInfo.UnzipDirectory.IsNullOrEmpty())
            archiveFileInfo.UnzipDirectory = options.UnzipDirectory;

        if (archiveFileInfo.UnzipDirectory.IsNullOrEmpty())
        {
            archiveFileInfo.UnzipDirectory = Path.Combine(Path.GetDirectoryName(archiveFileInfo.FilePath),
                               Path.GetFileNameWithoutExtension(archiveFileInfo.FilePath));
        }


        ArgumentNullException.ThrowIfNull(archive);

        if (archive.Entries.Any() != true)
            throw new UserFriendlyException("压缩文件中没有任何文件");

        if (options.CreateUnzipFolder)
            DirectoryHelper.CreateIfNotExists(archiveFileInfo.UnzipDirectory);


        var extractionOptions = new ExtractionOptions
        {
            ExtractFullPath = !options.NoKeepDirectoryStructure,
            Overwrite = options.DuplicateFileHandleType == DuplicateFileHandleType.Overwrite,
            PreserveFileTime = options.PreserveFileTime,
            PreserveAttributes = options.PreserveAttributes
        };


        var zipFileCount = archive.Entries.Count(x => !x.IsDirectory);
        var currentUnzipFileCount = 0;

        foreach (var archiveEntry in archive.Entries)
        {
            if (archiveEntry.IsDirectory) continue;

            archiveEntry.WriteToDirectory(archiveFileInfo.UnzipDirectory, extractionOptions);

            currentUnzipFileCount++;
            archiveFileInfo.ExtractProgress = (float)currentUnzipFileCount / zipFileCount;
        }

        // handler zip files
        UnzippedHandlerParts(archiveFileInfo, options);

        archiveFileInfo.ExtractProgress = 1;


        // scan inner zip files
        return options.UnzipInnerArchive ? await FindArchiveAsync(archiveFileInfo.UnzipDirectory, options, true) : [];
    }

    protected virtual void UnzippedHandlerParts(ArchiveFileInfo archiveFileInfo, UnzipOptions options)
    {
        archiveFileInfo.Archive?.Dispose();

        // 最新的 switch 语法处理 unzipPackageAfterHandleType
        switch (options.UnzipPackageAfterHandleType)
        {
            case UnzipPackageAfterHandleType.Delete:
                foreach (var part in archiveFileInfo.Parts)
                    try
                    {
                        FileHelper.DeleteIfExists(part);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                    }
                break;
            case UnzipPackageAfterHandleType.MoveToFolder:

                var unzipPackageMovePath = options.UnzipPackageMovePath;
                DirectoryHelper.CreateIfNotExists(unzipPackageMovePath);

                foreach (var part in archiveFileInfo.Parts)
                    File.Move(part, Path.Combine(unzipPackageMovePath, Path.GetFileName(part)));

                break;
            case UnzipPackageAfterHandleType.None:
                break;
        }

    }

    /// <summary>
    /// 检查是否匹配压缩文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="UserFriendlyException"></exception>
    protected virtual void CheckMatchArchiveFile(string filePath, List<string>? excludeRegexs = null,
        List<ArchiveType>? supportArchiveTypes = null)
    {
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("未找到文件。", filePath);

        var fileName = Path.GetFileName(filePath);
        if (!excludeRegexs.IsNullOrEmpty())
        {
            foreach (var excludeRegex in excludeRegexs)
            {
                if (Regex.IsMatch(fileName, excludeRegex))
                    throw new UserFriendlyException("文件名称被排除");
            }
        }

        var isArchive = ArchiveFactory.IsArchive(filePath, out var type);
        if (!isArchive) throw new UserFriendlyException("不是压缩文件");

        if (type == null)
            throw new UserFriendlyException("未知的压缩文件类型");

        if (!supportArchiveTypes.IsNullOrEmpty() && !supportArchiveTypes.Contains(type.Value))
            throw new UserFriendlyException("不支持的压缩文件类型");
    }

    /// <summary>
    /// 打开压缩文件
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="password"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual IArchive OpenArchiveUsePassword(IEnumerable<string> parts, string? password = null)
    {
        var partList = parts.ToList();
        if (partList.IsNullOrEmpty())
            throw new ArgumentException("值不可为空", nameof(parts));

        var files = partList.Select(x => new FileInfo(x));

        var readerOptions = new ReaderOptions();
        readerOptions.Password = password;

        return ArchiveFactory.Open(files, readerOptions);
    }

    protected virtual List<UnzipPassword> GetPasswords(bool choicePasswordOrderByUseCount)
    {
        var order = passwordRepository.GetAllPasswords().OrderBy(x => x.ManualSort);

        if (choicePasswordOrderByUseCount)
            order = order.ThenBy(x => x.UseCount);

        List<UnzipPassword> passwords =
            [null, .. order.ToList()];

        return passwords;
    }


}