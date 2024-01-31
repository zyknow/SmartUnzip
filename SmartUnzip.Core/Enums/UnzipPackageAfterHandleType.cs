using System.ComponentModel;

namespace SmartUnzip.Core.Enums;

/// <summary>
/// 解压完成后压缩包操作
/// </summary>
public enum UnzipPackageAfterHandleType
{
    /// <summary>
    ///  不做任何操作
    /// </summary>
    [Description("不做任何操作")]
    None,

    /// <summary>
    /// 移动到指定文件夹
    /// </summary>
    [Description("移动到指定文件夹")]
    MoveToFolder,

    /// <summary>
    /// 删除
    /// </summary>
    [Description("删除")]
    Delete
}