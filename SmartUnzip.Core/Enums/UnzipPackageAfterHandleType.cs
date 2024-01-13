namespace SmartUnzip.Core.Enums;

/// <summary>
/// 解压完成后压缩包操作
/// </summary>
public enum UnzipPackageAfterHandleType
{
    /// <summary>
    ///  不做任何操作
    /// </summary>
    None,

    /// <summary>
    /// 移动到指定文件夹
    /// </summary>
    MoveToFolder,

    /// <summary>
    /// 删除
    /// </summary>
    Delete
}