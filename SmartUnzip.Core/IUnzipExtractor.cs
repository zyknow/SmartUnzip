using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    Task ExtractsAsync(IEnumerable<IArchiveFileInfo> archiveFileInfo, IUnzipOptions options);

    /// <summary>
    /// 获取压缩文件信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="excludeRegexs"></param>
    /// <param name="supportArchiveTypes"></param>
    /// <returns></returns>
    IArchiveFileInfo GetArchiveFileInfo(string filePath,
        IEnumerable<string>? excludeRegexs = null,
        IEnumerable<ArchiveType>? supportArchiveTypes = null);

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(string directory, IUnzipOptions options,
        bool recursive = true);

    /// <summary>
    /// 扫描压缩文件
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="options"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(DirectoryInfo directory, IUnzipOptions options,
        bool recursive = true);

    Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(IEnumerable<FileInfo> files,
        IUnzipOptions options,
        bool recursive = true);

    Task<IEnumerable<IArchiveFileInfo>> FindArchiveAsync(IEnumerable<DirectoryInfo> directories,
        IUnzipOptions options,
        bool recursive = true);
}