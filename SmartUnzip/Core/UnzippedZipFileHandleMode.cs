using System.ComponentModel;

namespace SmartUnzip.Core;

public enum UnzippedZipFileHandleMode
{
    [Description("无操作")] None,
    [Description("删除")] Delete,
    [Description("移动到指定文件夹")] MoveToFolder
}