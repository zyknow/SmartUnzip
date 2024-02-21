using Bing.Extensions;
using Bing.IO;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SmartUnzip.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bing.Collections;
using Microsoft.Extensions.Options;

namespace SmartUnzip.Core;

public class DefaultUnzipExtractor(
    IPasswordRepository passwordRepository,
    ILogger<DefaultUnzipExtractor> logger,
    IServiceProvider serviceProvider,
    IOptions<SmartUnzipOptions> smartUnzipOptions,
    IUnzipUniqueCalculator unzipUniqueCalculator) : IUnzipExtractor
{
    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task ExtractsAsync(IEnumerable<IArchiveFileInfo> archiveFileInfo, IUnzipOptions options)
    {
        if (archiveFileInfo.IsEmpty())
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
    /// <returns></returns>
    public virtual IArchiveFileInfo GetArchiveFileInfo(string filePath,
        IEnumerable<string>? excludeRegexs = null,
        IEnumerable<ArchiveType>? supportArchiveTypes = null)
    {
        CheckMatchArchiveFile(filePath);

        var parts = ArchiveFactory.GetFileParts(filePath).ToList();

        if (parts.IsNullOrEmpty())
            throw new Exception("未找到压缩文件的所有分卷或本体");

        var archiveFileInfo = Activator.CreateInstance(smartUnzipOptions.Value.ArchiveFileInfoDefineType) as IArchiveFileInfo;

        archiveFileInfo.FilePath = parts.FirstOrDefault();
        archiveFileInfo.Parts = parts;


        return archiveFileInfo;
    }

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public virtual Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(string directory, IUnzipOptions options,
        bool recursive = true) => FindArchiveAsync(new DirectoryInfo(directory), options, recursive);

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(DirectoryInfo directory,
        IUnzipOptions options,
        bool recursive = true)
    {
        var files = directory.GetFiles().ToList();


        if (files.IsNullOrEmpty())
            return [];

        var infos = (await FindArchiveAsync(files, options, recursive)).ToList();

        if (!recursive) return infos;

        var subDirectories = directory.GetDirectories();
        foreach (var subDirectory in subDirectories)
        {
            infos.AddRange(await FindArchiveAsync(subDirectory, options, true));
        }

        return infos;
    }

    public virtual async Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(IEnumerable<DirectoryInfo> directories,
        IUnzipOptions options,
        bool recursive = true)
    {
        List<IArchiveFileInfo> infos = [];

        foreach (DirectoryInfo directoryInfo in directories)
        {
            infos.AddIfNotContains(await FindArchiveAsync(directoryInfo, options, recursive));
        }

        return infos;
    }

    public virtual async Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(IEnumerable<FileInfo> files,
        IUnzipOptions options,
        bool recursive = true)
    {
        List<IArchiveFileInfo> infos = [];

        if (files.IsEmpty())
            return [];

        foreach (var fileInfo in files.Where(f => options.ExcludePaths.All(e => e != f.FullName)))
        {
            // 排除已经处理过的文件
            if (infos.Any(info => info.Parts.Contains(fileInfo.FullName)))
                continue;

            IArchiveFileInfo? info = null;
            try
            {
                info = GetArchiveFileInfo(fileInfo.FullName,options.ExcludeRegexs,
                    options.SupportArchiveTypes);
                infos.Add(info);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }


        foreach (string s in files.Select(x => x.FullName))
            options.ExcludePaths.AddIfNotExist(s);

        return infos;
    }

    protected virtual async Task<IArchive?> OpenArchiveAsync(IArchiveFileInfo archiveFileInfo, IUnzipOptions options)
    {
        var lostParts = archiveFileInfo.Parts.Where(x => !File.Exists(x)).ToList();

        if (lostParts.Any())
            throw new FileNotFoundException("未找到文件。", lostParts.First());

        var passwords = GetPasswords(options.ChoicePasswordOrderByUseCount);

        Exception? ex = null;

        var readerOptions = new ReaderOptions();

        foreach (var unzipPassword in passwords)
        {
            try
            {
                readerOptions.Password = unzipPassword?.Value;

                archiveFileInfo.Password = unzipPassword?.Value;
                IArchive? archive = OpenArchiveAndTestPassword(archiveFileInfo.Parts, unzipPassword?.Value);
                archiveFileInfo.HasTestedPassword = true;

                if (unzipPassword != null)
                {
                    unzipPassword.IncreaseUseCount();
                    passwordRepository.UpdatePassword(unzipPassword);
                }

                return archive;
            }
            catch (CryptographicException e)
            {
                // logger.LogError(e, "密码错误。");

                ex = new CryptographicException(@$"密码 {unzipPassword?.Value} 错误");
                // archive?.Dispose();
            }
            catch (Exception e)
            {
                ex = e;
                // archive?.Dispose();

                // logger.LogError(e, "测试打开压缩文件异常。");
            }
        }

        if (ex is CryptographicException cryptographicException)
            ex = new CryptographicException("无正确密码");

        archiveFileInfo.HasTestedPassword = true;
        archiveFileInfo.Exception = ex;
        throw ex;
    }


    protected virtual async Task ExtractsAsync(IArchiveFileInfo info, SemaphoreSlim semaphore,
        IUnzipOptions options)
    {
        await semaphore.WaitAsync();
        try
        {
            // var children = await ExtractAsync(info, options);
            // info.Children = new ObservableCollection<IArchiveFileInfo>(children);

            await ExtractAsync(info, options);
        }
        catch (Exception e)
        {
            info.Exception = e;
            logger.LogError(e, e.Message);
            Gc();
        }
        finally
        {
            semaphore.Release();
        }

        foreach (IArchiveFileInfo childArchiveFileInfo in info.Children)
        {
            await ExtractsAsync(childArchiveFileInfo, semaphore, options);
        }
    }

    protected virtual async Task ExtractAsync(IArchiveFileInfo archiveFileInfo,
        IUnzipOptions options)
    {
        if (archiveFileInfo is null)
            throw new ArgumentNullException(nameof(archiveFileInfo));


        IArchive? archive = null;

        try
        {
            archive = await OpenArchiveAsync(archiveFileInfo, options);
        }
        catch (Exception e)
        {
            archiveFileInfo.Exception = e;
            throw;
        }


        SetUnzipDirectory(archiveFileInfo, options);

        ArgumentNullException.ThrowIfNull(archive);

        if (archive.Entries.Any() != true)
            throw new Exception("压缩文件中没有任何文件");

        // if (options.CreateUnzipFolder)
        DirectoryHelper.CreateIfNotExists(archiveFileInfo.UnzipDirectory);


        var extractionOptions = new ExtractionOptions
        {
            ExtractFullPath = !options.NoKeepDirectoryStructure,
            Overwrite = options.DuplicateFileHandleType == DuplicateFileHandleType.Overwrite,
            PreserveFileTime = options.PreserveFileTime,
            PreserveAttributes = options.PreserveAttributes,
        };


        var zipFileCount = archive.Entries.Count(x => !x.IsDirectory);
        var currentUnzipFileCount = 0;


        // archive.WriteToDirectory(archiveFileInfo.UnzipDirectory, extractionOptions);

        foreach (var archiveEntry in archive.Entries)
        {
            if (archiveEntry.IsDirectory) continue;

            try
            {
                var unzipFilePath = Path.Combine(archiveFileInfo.UnzipDirectory, archiveEntry.Key);
                if (options.DuplicateFileHandleType == DuplicateFileHandleType.Rename)
                    unzipFilePath = unzipUniqueCalculator.GetUniqueFileName(unzipFilePath, options);

                archiveEntry.WriteToFile(unzipFilePath, extractionOptions);
            }
            catch (NotImplementedException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            currentUnzipFileCount++;
            archiveFileInfo.ExtractProgress = (float) currentUnzipFileCount / zipFileCount;
        }

        // handler zip files
        archive?.Dispose();
        Gc();
        UnzippedHandlerParts(archiveFileInfo, options);

        archiveFileInfo.ExtractProgress = 1;


        var innerArchiveFileInfos = await FindArchiveAsync(archiveFileInfo.UnzipDirectory, options);

        archiveFileInfo.Children.Clear();

        archiveFileInfo.Children.AddIfNotContains(innerArchiveFileInfos);


        foreach (IArchiveFileInfo fileInfo in archiveFileInfo.Children)
        {
            try
            {
                await ExtractAsync(fileInfo, options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        // scan inner zip files
        // return options.UnzipInnerArchive ? await FindArchiveAsync(archiveFileInfo.UnzipDirectory, options) : [];
    }

    protected virtual void Gc()
    {
        GC.Collect(2);
        GC.WaitForPendingFinalizers();
    }

    protected virtual void SetUnzipDirectory(IArchiveFileInfo archiveFileInfo, IUnzipOptions options)
    {
        if (!archiveFileInfo.UnzipDirectory.IsEmpty())
            return;

        if (!options.UnzipDirectory.IsEmpty())
        {
            archiveFileInfo.UnzipDirectory = options.UnzipDirectory;
        }
        else
        {
            archiveFileInfo.UnzipDirectory = Path.Combine(Path.GetDirectoryName(archiveFileInfo.FilePath),
                Path.GetFileNameWithoutExtension(archiveFileInfo.FilePath));
        }

        if (options.NoKeepDirectoryStructure)
        {
            archiveFileInfo.UnzipDirectory = Path.GetDirectoryName(archiveFileInfo.UnzipDirectory);
        }
    }

    protected virtual void UnzippedHandlerParts(IArchiveFileInfo archiveFileInfo, IUnzipOptions options)
    {
        // 最新的 switch 语法处理 unzipPackageAfterHandleType
        switch (options.UnzipPackageAfterHandleType)
        {
            case UnzipPackageAfterHandleType.Delete:
                foreach (var part in archiveFileInfo.Parts)
                {
                    try
                    {
                        FileHelper.Delete(part);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, e.Message);
                    }
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
    protected virtual void CheckMatchArchiveFile(string filePath, IEnumerable<string>? excludeRegexs = null,
        IEnumerable<ArchiveType>? supportArchiveTypes = null)
    {
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("未找到文件。", filePath);

        var fileName = Path.GetFileName(filePath);
        if (!excludeRegexs.IsEmpty())
        {
            foreach (var excludeRegex in excludeRegexs)
            {
                if (Regex.IsMatch(fileName, excludeRegex))
                    throw new Exception("文件名称被排除");
            }
        }

        var isArchive = ArchiveFactory.IsArchive(filePath, out var type);
        if (!isArchive) throw new Exception(@$"不是压缩文件 :{filePath}");

        if (type == null)
            throw new Exception("未知的压缩文件类型");

        if (!supportArchiveTypes.IsEmpty() && !supportArchiveTypes.Contains(type.Value))
            throw new Exception("不支持的压缩文件类型");
    }

    /// <summary>
    /// 打开压缩文件
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="password"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual IArchive OpenArchiveAndTestPassword(IEnumerable<string> filePaths, string? password = null)
    {
        if (filePaths.IsEmpty())
            throw new ArgumentException("值不可为空", nameof(filePaths));

        var files = filePaths.Select(x => new FileInfo(x)).ToList();


        var readerOptions = new ReaderOptions();
        readerOptions.Password = password;

        var archive = ArchiveFactory.Open(files, readerOptions);


        // 测试打开流，打不开说明密码错误
        using var steam = archive.Entries.First().OpenEntryStream();
        //
        // using var steams = new MemoryStream();
        // archive.Entries.First().WriteTo(steams);
        
        Console.WriteLine("成功");

        // if (password.IsEmpty() && archive.Entries.Any(x => x.IsEncrypted))
        // {
        //     
        //     var source = archive.Entries.OrderBy(x => x.Size).FirstOrDefault((x => x.IsEncrypted));
        //
        //     // 测试打开流
        //     using var steam = source.OpenEntryStream();
        //     
        //     // if (password.IsEmpty())
        //     //     throw new CryptographicException("压缩文件需要密码");
        //     // else
        //     // {
        //     //     var source = archive.Entries.OrderBy(x => x.Size).FirstOrDefault((x => x.IsEncrypted));
        //     //
        //     //     // 测试打开流
        //     //     using var steam = source.OpenEntryStream();
        //     // }
        // }

        return archive;
    }

    protected virtual List<IUnzipPassword> GetPasswords(bool choicePasswordOrderByUseCount)
    {
        var order = passwordRepository.GetAllPasswords().OrderBy(x => x.ManualSort);

        if (choicePasswordOrderByUseCount)
            order = order.ThenBy(x => x.UseCount);

        List<IUnzipPassword> passwords =
            [null, .. order.ToList()];

        return passwords;
    }
}