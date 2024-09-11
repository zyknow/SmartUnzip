namespace SmartUnzip.Core;

public class ArchiveFileSearchResult
{
    public string FilePath { get; set; }
    public bool IsVolume { get; set; }
    public string ServenZOuput { get; set; }
}

public class ArchiveFileValidResult : ArchiveFileSearchResult
{
    public bool IsValid { get; set; }
}