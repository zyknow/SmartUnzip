namespace SmartUnzip.Core;

public partial class UnzipOptions : ObservableObject
{
    public DuplicateFileHandleMode DuplicateFileHandleMode { get; set; } = DuplicateFileHandleMode.Skip;

    public bool UnzipSortByPasswordUseCount { get; set; } = true;

    public string ExcludeSearchFileExtensions { get; set; } = "EPUB,ISO,IMG,APK,JAR,XLSX,PPTX,DOCX,downloading";

    public UnzipCreateSeparateMode UnzipCreateSeparateMode { get; set; } =
        UnzipCreateSeparateMode.NotCreateOnSingleFolderOrFile;

    [ObservableProperty]
    string? _outputPath;

    // public bool NotKeepDirectoryStructure { get; set; }

    public bool RecursiveUnzip { get; set; } = true;

    [ObservableProperty]
    bool _unzippedDeleteArchiveFile;

    public string UnzippedDeleteFileRegexs { get; set; } = "";
    public string UnzippedDeleteFolderRegexs { get; set; } = "";


    bool _singleSameNameFolderMoveUp;
    
    public bool SingleSameNameFolderMoveUp
    {
        get => _singleSameNameFolderMoveUp;
        set
        {
            if (value == _singleSameNameFolderMoveUp)
            {
                return;
            }

            _singleSameNameFolderMoveUp = value;

            if (value)
            {
                UnzippedDeleteArchiveFile = true;
            }
            
            OnPropertyChanged();
        }
    }
}