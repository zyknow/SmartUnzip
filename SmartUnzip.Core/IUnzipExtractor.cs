namespace SmartUnzip.Core;

public interface IUnzipExtractor
{
    IAsyncEnumerable<ArchiveFileInfo> ExtractsAsync(List<ArchiveFileInfo> archiveFileInfo);
    Task ExtractAsync(ArchiveFileInfo archiveFileInfo);
    void TestedOpenArchive(ArchiveFileInfo archiveFileInfo);
}