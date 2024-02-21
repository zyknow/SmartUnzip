using System;
using System.Collections.Generic;
using SharpCompress.Common;
using SmartUnzip.Core.Enums;

namespace SmartUnzip.Core;

public interface IUnzipOptions
{
    /// <summary>
    /// 保存文件时间
    /// </summary>
    bool PreserveFileTime { get; set; }

    /// <summary>
    /// 保存Windows文件属性
    /// </summary>
    bool PreserveAttributes { get; set; }

    /// <summary>
    /// 不保留解压文件结构
    /// </summary>
    bool NoKeepDirectoryStructure { get; set; }

    /// <summary>
    /// 解压压缩包内的解压文件
    /// </summary>
    bool UnzipInnerArchive { get; set; }

    /// <summary>
    /// 是否按使用次数排序密码。
    /// </summary>
    bool ChoicePasswordOrderByUseCount { get; set; }

    /// <summary>
    /// 调试模式
    /// </summary>
    bool DebuggerMode { get; set; }

    /// <summary>
    /// 最大解压压缩包并发数
    /// </summary>
    int MaxUnzipArchiveCount { get; set; }

    /// <summary>
    /// 统一解压到指定路径
    /// </summary>
    string? UnzipDirectory { get; set; }

    /// <summary>
    ///  解压包后处理方式
    /// </summary>
    UnzipPackageAfterHandleType UnzipPackageAfterHandleType { get; set; }

    /// <summary>
    /// 解压包移动路径
    /// </summary>
    string UnzipPackageMovePath { get; set; }

    /// <summary>
    /// 重复文件的处理方式。
    /// </summary>
    DuplicateFileHandleType DuplicateFileHandleType { get; set; }

    /// <summary>
    /// 包含的正则表达式列表。解压时，只有符合这些正则表达式的文件才会被解压。
    /// </summary>
    ICollection<string> IncludeRegexs { get; }

    /// <summary>
    /// 排除的正则表达式列表。解压时，符合这些正则表达式的文件将不会被解压。
    /// </summary>
    ICollection<string> ExcludeRegexs { get; }

    /// <summary>
    /// 支持的压缩文件类型列表。只有这些类型的压缩文件才会被解压。
    /// </summary>
    ICollection<ArchiveType> SupportArchiveTypes { get; }

    /// <summary>
    /// 已排除的压缩包路径
    /// </summary>
    public ICollection<string> ExcludePaths { get; }

    void ResetExcludePaths();
}