using System;
using System.Collections.Generic;

namespace SmartUnzip.Core;

/// <summary>
/// 定义存档文件信息的接口。
/// </summary>
public interface IArchiveFileInfo
{
    /// <summary>
    /// 获取或设置文件路径。
    /// </summary>
    string FilePath { get; set; }

    /// <summary>
    /// 获取存档文件的各个部分。
    /// </summary>
    List<string> Parts { get; set; }

    /// <summary>
    /// 获取或设置存档文件的密码。
    /// </summary>
    string Password { get; set; }

    /// <summary>
    /// 获取或设置密码是否已经测试。
    /// </summary>
    bool HasTestedPassword { get; set; }

    /// <summary>
    /// 获取或设置在处理文件时发生的异常信息。
    /// </summary>
    Exception? Exception { get; set; }

    /// <summary>
    /// 获取或设置解压百分比进度。
    /// </summary>
    float ExtractProgress { get; set; }

    /// <summary>
    /// 获取或设置文件解压的目标目录。
    /// </summary>
    string UnzipDirectory { get; set; }

    public ICollection<IArchiveFileInfo> Children { get; }
}