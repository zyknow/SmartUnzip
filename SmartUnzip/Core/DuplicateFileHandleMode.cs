using System.ComponentModel;

namespace SmartUnzip.Core;

public enum DuplicateFileHandleMode
{
    [Description("跳过")] Skip,
    [Description("覆盖")] Overwrite,
    [Description("重命名")] Rename
}