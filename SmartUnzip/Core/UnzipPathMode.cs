using System.ComponentModel;

namespace SmartUnzip.Core;

public enum UnzipPathMode
{
     [Description("原地解压")]
     Local,
     [Description("解压至统一文件夹")]
     AssignFolder
}