namespace SmartUnzip.Core;

public interface IArchiveFinder
{
    public Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(string directory, bool recursive = false);
    public Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(DirectoryInfo directory, bool recursive = false);
}