using System.ComponentModel;

namespace SmartUnzip.Core.Enums;

/// <summary>
///  重复文件处理方式
/// </summary>
public enum DuplicateFileHandleType
{
    /// <summary>
    /// 覆盖
    /// </summary>
    [Description("覆盖")]
    Overwrite,

    /// <summary>
    /// 跳过
    /// </summary>
    [Description("跳过")]
    Skip,

    /// <summary>
    /// 重命名
    /// </summary>
    [Description("重命名")]
    Rename
}