using SharpCompress.Common;

namespace SmartUnzip.Core;

public interface IUnzipExtractor
{
    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="archiveFileInfo"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    Task ExtractsAsync(List<ArchiveFileInfo> archiveFileInfo, UnzipOptions options);

    /// <summary>
    /// 获取压缩文件信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="UserFriendlyException"></exception>
    /// <returns></returns>
    ArchiveFileInfo GetArchiveFileInfo(string filePath, List<string>? excludeRegexs = null,
        List<ArchiveType>? supportArchiveTypes = null);

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(string directory, UnzipOptions options,
        bool recursive = true);

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(DirectoryInfo directory, UnzipOptions options,
        bool recursive = true);
}