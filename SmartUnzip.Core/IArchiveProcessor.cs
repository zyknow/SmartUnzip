using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace SmartUnzip.Core;

public interface IArchiveProcessor
{
    /// <summary>
    /// 检查是否匹配压缩文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="UserFriendlyException"></exception>
    void CheckMatchArchiveFile(string filePath, List<string>? excludeRegexs = null,
        List<ArchiveType>? supportArchiveTypes = null);

    /// <summary>
    /// 获取压缩文件信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="UserFriendlyException"></exception>
    /// <returns></returns>
    ArchiveFileInfo GetArchiveFileInfo(string filePath, List<string>? excludeRegexs = null,
        List<ArchiveType>? supportArchiveTypes = null);

    /// <summary>
    ///  打开压缩文件
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="readerOptionsAction"></param>
    /// <exception cref="ArgumentNullException"></exception>
    IArchive OpenArchive(IEnumerable<string> parts, Action<ReaderOptions>? readerOptionsAction = null);

    /// <summary>
    /// 打开压缩文件
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="password"></param>
    /// <exception cref="ArgumentNullException"></exception>
    IArchive OpenArchive(IEnumerable<string> parts, string password);

    void Extractor(ArchiveFileInfo archiveFileInfo,
        Action<ExtractionOptions>? extractionOptionsAction = null);
}