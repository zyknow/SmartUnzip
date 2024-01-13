using SharpCompress.Common;
using SmartUnzip.Core.Enums;

namespace SmartUnzip.Core.Datas;

public class UnzipConsts
{
    public const UnzipPackageAfterHandleType DefaultUnzipPackageAfterHandlerType = UnzipPackageAfterHandleType.None;

    public const DuplicateFileHandleType DefaultDuplicateFileHandleType = DuplicateFileHandleType.Overwrite;

    public static List<ArchiveType> DefaultSupportArchiveTypes { get; } =
    [
        ArchiveType.Rar,
        ArchiveType.Zip,
        ArchiveType.Tar,
        ArchiveType.SevenZip,
        ArchiveType.GZip
    ];

    public static List<string> DefaultIncludeRegexs { get; } =
    [
        @"^.*\.(txt|doc|docx|xls|xlsx|ppt|pptx|pdf|jpg|jpeg|png|gif|bmp|tif|tiff|zip|rar|7z|gz|tar)$"
    ];

    public static List<string> DefaultExcludeRegexs { get; } =
        [];
}