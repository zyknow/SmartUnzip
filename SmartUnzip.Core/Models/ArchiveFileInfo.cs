using SharpCompress.Archives;

namespace SmartUnzip.Core.Models;

/// <summary>
/// 存档文件信息
/// </summary>
public class ArchiveFileInfo : IDisposable
{
    /// <summary>
    /// 如果Parts的数量大于1，表示这是一个多部分的存档
    /// </summary>
    public bool IsMulti => Parts.Count > 1;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 文件的各个部分，如果是多部分存档，这个列表会包含多个元素
    /// </summary>
    public List<string> Parts { get; set; } = [];


    public string Password { get; set; }

    public bool TestedPassword { get; set; }

    public IArchive Archive { get; set; }

    /// <summary>
    /// 如果在处理文件时发生异常，这个属性会包含异常信息
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// 解压百分比进度
    /// </summary>
    public float ExtractProgress { get; set; }

    /// <summary>
    /// 文件解压的目标目录
    /// </summary>
    public string ExtractDirectory { get; set; }


    public void Dispose()
    {
        Archive?.Dispose();
    }
}