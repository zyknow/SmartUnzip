using System.ComponentModel;

namespace SmartUnzip.Core;

public enum UnzipCreateSeparateMode
{
    [Description("创建")] Create,
    [Description("不在单个文件夹或文件上创建")] NotCreateOnSingleFolderOrFile
}