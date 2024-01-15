using Shouldly;
using SmartUnzip.Core.Tests.Consts;

namespace SmartUnzip.Core.Tests.Core;

public sealed class ArchiveFinderTests : SmartUnzipCoreTestBase
{
    private readonly IArchiveFinder _archiveFinder;



    public ArchiveFinderTests()
    {
        _archiveFinder = GetRequiredService<IArchiveFinder>();
    }

    [Fact]
    public async Task FindArchiveAsync_ShouldReturnCorrectArchiveFiles()
    {
        // Arrange
        var expectedFiles = SmartUnzipCoreTestConsts.TestFiles.Select(Path.GetFullPath).ToList();

        // Act
        var result = (await _archiveFinder.FindArchiveAsync(SmartUnzipCoreTestConsts.TestDirectory, true)).ToList();

        // Assert
        result.ShouldNotBeNull();

        foreach (var part in result.SelectMany(archiveFileInfo => archiveFileInfo.Parts))
        {
            expectedFiles.ShouldContain(part);
        }
    }
}