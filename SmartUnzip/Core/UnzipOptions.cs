namespace SmartUnzip.Core;

public partial class UnzipOptions : ObservableObject
{
    public DuplicateFileHandleMode DuplicateFileHandleMode { get; set; } = DuplicateFileHandleMode.Skip;

    public bool UnzipSortByPasswordUseCount { get; set; } = true;

    public string IncludeExtensions { get; set; } = "zip,rar,7z,tar";

    public string ExcludeExtensions { get; set; } = "EPUB,ISO,IMG,APK,JAR,XLSX,PPTX,DOCX,downloading";

    public string SkipUnzippedSearchFileOrFolders { get; set; } = "";

    // public bool CreateSeparateFolder { get; set; } = true;

    public UnzipCreateSeparateMode UnzipCreateSeparateMode { get; set; } =
        UnzipCreateSeparateMode.NotCreateOnSingleFolderOrFile;

    [ObservableProperty] string? _outputPath;

    public bool NotKeepDirectoryStructure { get; set; }

    public bool RecursiveUnzip { get; set; } = true;

    public bool UnzippedDeleteArchiveFile { get; set; }
}