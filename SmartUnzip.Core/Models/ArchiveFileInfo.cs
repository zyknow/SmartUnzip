using CommunityToolkit.Mvvm.ComponentModel;
using SharpCompress.Archives;
using SmartUnzip.Core.Enums;
using System.Collections.ObjectModel;

namespace SmartUnzip.Core.Models;

/// <summary>
/// 存档文件信息
/// </summary>
public partial class ArchiveFileInfo : ObservableObject, IDisposable
{
    /// <summary>
    /// 如果Parts的数量大于1，表示这是一个多部分的存档
    /// </summary>
    public bool IsMulti => Parts.Count > 1;

    public string Name => Path.GetFileName(FilePath);

    /// <summary>
    /// 解压完成
    /// </summary>
    public bool Unzipped => (int)ExtractProgress == 1;

    /// <summary>
    /// 文件路径
    /// </summary>
    [ObservableProperty]
    private string _filePath;
    /// <summary>
    /// 文件的各个部分，如果是多部分存档，这个列表会包含多个元素
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMulti))]
    private List<string> _parts = [];

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private bool _hasTestedPassword;

    [ObservableProperty]
    private IArchive? _archive;

    /// <summary>
    /// 如果在处理文件时发生异常，这个属性会包含异常信息
    /// </summary>
    [ObservableProperty]
    private Exception _exception;

    /// <summary>
    /// 解压百分比进度
    /// </summary>
    [ObservableProperty]
    private float _extractProgress;

    /// <summary>
    /// 文件解压的目标目录
    /// </summary>
    [ObservableProperty]
    private string _unzipDirectory;

    [ObservableProperty]
    private ObservableCollection<ArchiveFileInfo> _children = [];

    public void Dispose()
    {
        Archive?.Dispose();
    }
}