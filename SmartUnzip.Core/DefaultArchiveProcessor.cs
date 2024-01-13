using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using Volo.Abp.IO;

namespace SmartUnzip.Core;

public class DefaultArchiveProcessor(
    ILogger<DefaultArchiveProcessor> logger)
    : IArchiveProcessor, ITransientDependency
{
    /// <summary>
    /// 检查是否匹配压缩文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="UserFriendlyException"></exception>
    public virtual void CheckMatchArchiveFile(string filePath, List<string>? excludeRegexs = null,
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
    ///  打开压缩文件
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="readerOptionsAction"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual IArchive OpenArchive(IEnumerable<string> parts, Action<ReaderOptions>? readerOptionsAction = null)
    {
        var partList = parts.ToList();
        if (partList.IsNullOrEmpty())
            throw new ArgumentException("值不可为空", nameof(parts));

        var files = partList.Select(x => new FileInfo(x));

        var readerOptions = new ReaderOptions();
        readerOptionsAction?.Invoke(readerOptions);

        return ArchiveFactory.Open(files, readerOptions);
    }

    /// <summary>
    /// 打开压缩文件
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="password"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual IArchive OpenArchive(IEnumerable<string> parts, string password)
    {
        return OpenArchive(parts, opt => opt.Password = password);
    }

    public virtual void Extractor(ArchiveFileInfo archiveFileInfo,
        Action<ExtractionOptions>? extractionOptionsAction = null)
    {
        var archive = archiveFileInfo.Archive;

        if (archive is null)
            throw new UserFriendlyException("未打开压缩文件");

        if (archiveFileInfo.ExtractDirectory.IsNullOrEmpty())
            throw new UserFriendlyException("未指定解压目录");


        ArgumentNullException.ThrowIfNull(archive);

        if (archive.Entries.Any() != true)
            throw new UserFriendlyException("压缩文件中没有任何文件");

        var extractionOptions = new ExtractionOptions();
        extractionOptionsAction?.Invoke(extractionOptions);


        DirectoryHelper.CreateIfNotExists(archiveFileInfo.ExtractDirectory);

        var zipFileCount = archive.Entries.Count(x => !x.IsDirectory);
        var currentUnzipFileCount = 0;

        
        foreach (var archiveEntry in archive.Entries)
        {
            if (archiveEntry.IsDirectory) continue;

            Console.WriteLine(archiveEntry.Key);
            archiveEntry.WriteToDirectory(archiveFileInfo.ExtractDirectory, extractionOptions);

            currentUnzipFileCount++;
            archiveFileInfo.ExtractProgress = (float) currentUnzipFileCount / zipFileCount;
        }
    }
}