using Shouldly;
using SmartUnzip.Core.Models;
using SmartUnzip.Core.Tests.Consts;

namespace SmartUnzip.Core.Tests.Core;

public sealed class ArchiveProcessorTests : SmartUnzipCoreTestBase
{
    private readonly IArchiveProcessor _archiveProcessor;
    private readonly IPasswordRepository _passwordRepository;

    public ArchiveProcessorTests()
    {
        _archiveProcessor = GetRequiredService<IArchiveProcessor>();
        _passwordRepository = GetRequiredService<IPasswordRepository>();

        _passwordRepository.AddPassword(new UnzipPassword(SmartUnzipCoreTestConsts.DefaultPassword));
    }


    [Fact]
    public void CheckMatchArchiveFile_ShouldNotThrowException_WhenFileIsArchive()
    {
        // Arrange
        var filePath = SmartUnzipCoreTestConsts.TestFiles.First();

        // Act & Assert
        Should.NotThrow(() => _archiveProcessor.CheckMatchArchiveFile(filePath));
    }

    [Fact]
    public void GetArchiveFileInfo_ShouldReturnCorrectInfo_WhenFileIsArchive()
    {
        // Arrange
        var filePath = SmartUnzipCoreTestConsts.TestFiles.First();

        // Act
        var archiveFileInfo = _archiveProcessor.GetArchiveFileInfo(filePath);
        archiveFileInfo.Archive = _archiveProcessor.OpenArchive(archiveFileInfo.Parts);

        // Assert
        var fileFullPath = Path.GetFullPath(filePath);

        archiveFileInfo.ShouldNotBeNull();
        archiveFileInfo.FilePath.ShouldBe(fileFullPath);
        archiveFileInfo.Parts.Count.ShouldBe(1);
        archiveFileInfo.Parts.First().ShouldBe(fileFullPath);
    }

    [Fact]
    public void OpenArchive_ShouldReturnArchive_WhenFileIsArchive()
    {
        // Arrange
        var filePath = SmartUnzipCoreTestConsts.TestFiles.First();
        var parts = new List<string> { filePath };

        // Act
        var archive = _archiveProcessor.OpenArchive(parts);

        // Assert
        archive.ShouldNotBeNull();
    }

    [Fact]
    public async Task Extractor_ShouldExtractFiles_WhenFileIsArchive()
    {
        // Arrange
        var filePath = SmartUnzipCoreTestConsts.TestFiles.First();
        var extractDirectory = Path.GetFullPath(Path.Combine(SmartUnzipCoreTestConsts.TestDirectory, "extracted"));
        var archiveFileInfo = _archiveProcessor.GetArchiveFileInfo(filePath);
        archiveFileInfo.ExtractDirectory = extractDirectory;

        archiveFileInfo.Archive =
            _archiveProcessor.OpenArchive(archiveFileInfo.Parts, SmartUnzipCoreTestConsts.DefaultPassword);
        archiveFileInfo.Archive.ShouldNotBeNull();

        // Act
        _archiveProcessor.Extractor(archiveFileInfo, opt => opt.Overwrite = true);

        // Assert
        Directory.Exists(extractDirectory).ShouldBeTrue();
        Directory.GetFiles(extractDirectory).ShouldNotBeEmpty();

        Directory.Delete(extractDirectory, true);
    }
}