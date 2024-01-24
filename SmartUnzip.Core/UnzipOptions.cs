using SharpCompress.Common;
using SmartUnzip.Core.Datas;
using SmartUnzip.Core.Enums;
using System.ComponentModel;

namespace SmartUnzip.Core;

public class UnzipOptions
{
    /// <summary>
    /// 保存文件时间
    /// </summary>
    public bool PreserveFileTime { get; set; }

    /// <summary>
    /// 保存Windows文件属性
    /// </summary>
    public bool PreserveAttributes { get; set; } = true;


    /// <summary>
    /// 不保留解压文件结构
    /// </summary>
    public bool NoKeepDirectoryStructure { get; set; } = false;

    /// <summary>
    /// 解压压缩包内的解压文件
    /// </summary>
    public bool UnzipInnerArchive { get; set; } = true;

    /// <summary>
    /// 创建解压文件夹
    /// </summary>
    // public bool CreateUnzipFolder { get; set; } = true;


    /// <summary>
    /// 是否按使用次数排序密码。
    /// </summary>
    public bool ChoicePasswordOrderByUseCount { get; set; } = true;

    /// <summary>
    /// 调试模式
    /// </summary>
    public bool DebuggerMode { get; set; }

    ///// <summary>
    ///// 并发测试密码
    ///// </summary>
    //public bool ConcurrentTestPassword { get; set; }

    /// <summary>
    /// 最大解压压缩包并发数
    /// </summary>
    public int MaxUnzipArchiveCount { get; set; } = 5;

    /// <summary>
    /// 统一解压到指定路径
    /// </summary>
    public string? UnzipDirectory { get; set; }

    /// <summary>
    ///  解压包后处理方式
    /// </summary>
    public UnzipPackageAfterHandleType UnzipPackageAfterHandleType { get; set; } =
        UnzipConsts.DefaultUnzipPackageAfterHandlerType;

    /// <summary>
    /// 解压包移动路径
    /// </summary>
    public string UnzipPackageMovePath { get; set; }

    /// <summary>
    /// 重复文件的处理方式。
    /// </summary>
    public DuplicateFileHandleType DuplicateFileHandleType { get; set; } = UnzipConsts.DefaultDuplicateFileHandleType;

    /// <summary>
    /// 包含的正则表达式列表。解压时，只有符合这些正则表达式的文件才会被解压。
    /// </summary>
    public List<string> IncludeRegexs { get; set; } = UnzipConsts.DefaultIncludeRegexs;

    /// <summary>
    /// 排除的正则表达式列表。解压时，符合这些正则表达式的文件将不会被解压。
    /// </summary>
    public List<string> ExcludeRegexs { get; set; } = UnzipConsts.DefaultExcludeRegexs;

    /// <summary>
    /// 支持的压缩文件类型列表。只有这些类型的压缩文件才会被解压。
    /// </summary>
    public List<ArchiveType> SupportArchiveTypes { get; set; } = UnzipConsts.DefaultSupportArchiveTypes;

    internal HashSet<string> ExcludePaths { get; set; } = [];

    public void ResetExcludePaths()
    {
        ExcludePaths = [];
    }
}