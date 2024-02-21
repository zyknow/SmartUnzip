using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SharpCompress.Common;
using SmartUnzip.Avalonia.Datas;
using SmartUnzip.Core;
using SmartUnzip.Core.Enums;

namespace SmartUnzip.Avalonia;

public partial class UnzipOptions : ObservableObject, IUnzipOptions
{
    /// <summary>
    /// 保存文件时间
    /// </summary>
    [ObservableProperty]
    bool _preserveFileTime;

    /// <summary>
    /// 保存Windows文件属性
    /// </summary>
    [ObservableProperty]
    bool _preserveAttributes = true;

    /// <summary>
    /// 不保留解压文件结构
    /// </summary>
    [ObservableProperty]
    bool _noKeepDirectoryStructure = false;

    /// <summary>
    /// 解压压缩包内的解压文件
    /// </summary>
    [ObservableProperty]
    bool _unzipInnerArchive = true;

    /// <summary>
    /// 是否按使用次数排序密码。
    /// </summary>
    [ObservableProperty]
    bool _choicePasswordOrderByUseCount = true;

    /// <summary>
    /// 调试模式
    /// </summary>
    [ObservableProperty]
    bool _debuggerMode;

    /// <summary>
    /// 最大解压压缩包并发数
    /// </summary>
    [ObservableProperty]
    int _maxUnzipArchiveCount = 5;

    /// <summary>
    /// 统一解压到指定路径
    /// </summary>
    [ObservableProperty]
    string? _unzipDirectory;

    /// <summary>
    /// 解压包后处理方式
    /// </summary>
    [ObservableProperty]
    UnzipPackageAfterHandleType _unzipPackageAfterHandleType = UnzipConsts.DefaultUnzipPackageAfterHandlerType;

    /// <summary>
    /// 解压包移动路径
    /// </summary>
    [ObservableProperty]
    string? _unzipPackageMovePath;

    /// <summary>
    /// 重复文件的处理方式。
    /// </summary>
    [ObservableProperty]
    DuplicateFileHandleType _duplicateFileHandleType = UnzipConsts.DefaultDuplicateFileHandleType;

    /// <summary>
    /// 包含的正则表达式列表。解压时，只有符合这些正则表达式的文件才会被解压。
    /// </summary>
    [ObservableProperty]
    ICollection<string> _includeRegexs = new ObservableCollection<string>(UnzipConsts.DefaultIncludeRegexs);

    /// <summary>
    /// 排除的正则表达式列表。解压时，符合这些正则表达式的文件将不会被解压。
    /// </summary>
    [ObservableProperty]
    ICollection<string> _excludeRegexs = new ObservableCollection<string>(UnzipConsts.DefaultExcludeRegexs);

    /// <summary>
    /// 支持的压缩文件类型列表。只有这些类型的压缩文件才会被解压。
    /// </summary>
    [ObservableProperty]
    ICollection<ArchiveType> _supportArchiveTypes = new ObservableCollection<ArchiveType>(UnzipConsts.DefaultSupportArchiveTypes);

    HashSet<string> _excludePaths = [];

    public ICollection<string> ExcludePaths
    {
        get => _excludePaths;
    }


    public void ResetExcludePaths()
    {
        _excludePaths = [];
    }
}