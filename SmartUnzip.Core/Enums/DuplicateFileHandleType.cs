namespace SmartUnzip.Core.Enums;

/// <summary>
///  重复文件处理方式
/// </summary>
public enum DuplicateFileHandleType
{
    /// <summary>
    /// 覆盖
    /// </summary>
    Overwrite,

    /// <summary>
    /// 跳过
    /// </summary>
    Skip,

    /// <summary>
    /// 重命名
    /// </summary>
    Rename
}