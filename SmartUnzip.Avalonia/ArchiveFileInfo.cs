using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bing.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartUnzip.Core;

namespace SmartUnzip.Avalonia;

/// <summary>
/// 存档文件信息
/// </summary>
public partial class ArchiveFileInfo : ObservableObject, IArchiveFileInfo
{
    /// <summary>
    /// 如果Parts的数量大于1，表示这是一个多部分的存档
    /// </summary>
    public bool IsMulti => Parts.Count > 1;

    public string Name => Path.GetFileName((string?) FilePath);

    /// <summary>
    /// 解压完成
    /// </summary>
    public bool Unzipped => (int) ExtractProgress == 1;

    public bool ShowException => Exception != null;

    public bool ShowPassword => Exception == null && !BingExtensions.IsEmpty((string) Password);

    /// <summary>
    /// 文件路径
    /// </summary>
    [ObservableProperty]
    private string _filePath = "";

    /// <summary>
    /// 文件的各个部分，如果是多部分存档，这个列表会包含多个元素
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMulti))]
    private List<string> _parts = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPassword))]
    private string _password = "";

    [ObservableProperty]
    private bool _hasTestedPassword;

    /// <summary>
    /// 如果在处理文件时发生异常，这个属性会包含异常信息
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowException))]
    private Exception? _exception;

    /// <summary>
    /// 解压百分比进度
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Unzipped))]
    private float _extractProgress;

    /// <summary>
    /// 文件解压的目标目录
    /// </summary>
    [ObservableProperty]
    private string? _unzipDirectory;

    [ObservableProperty]
    private ICollection<IArchiveFileInfo> _children = new ObservableCollection<IArchiveFileInfo>();

    public override bool Equals(object? obj)
    {
        if (obj is ArchiveFileInfo archiveFileInfo)
        {
            return archiveFileInfo.FilePath == FilePath;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return FilePath.GetHashCode();
    }
}