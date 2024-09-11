using SmartUnzip.Core;
using SmartUnzip.Services;

namespace SmartUnzip.Tests;

public class SevenZHelperTests
{
    readonly ArchiveService _archiveService = new ArchiveService();

    public SevenZHelperTests()
    {
        App.Settings = new SettingsOptions();
        App.Settings.SevenZFilePath = @"C:\App Files\7z\7z.exe";
    }


    [Fact]
    public async void TestArchive()
    {
       var res =  await _archiveService.TestArchiveAsync(@$"E:\Downloads\test-archive\230\2.7z");
    }
    
    [Fact]
    public async void TestExtractWith7Zip()
    {
        var archivePath = @$"TestPackages\volume.zip";
        var outputPath = @$"TestPackages\TestExtractWith7Zip";

        await _archiveService.ExtractAsync(archivePath,
            outputPath);

        Assert.True(Directory.Exists(outputPath));

        // Directory.Delete(unzipPath, true);
    }

    [Fact]
    public async void TestArchivePassword()
    {
        var archivePath = @$"TestPackages\password-123.zip";
        var password = "123";

        Assert.True(await _archiveService.TestArchivePasswordAsync(archivePath, password));
    }

    [Fact]
    public async void TestArchivePasswordShouldFailed()
    {
        var archivePath = @$"TestPackages\password-123.zip";
        var password = "";

        Assert.False(await _archiveService.TestArchivePasswordAsync(archivePath, password));
    }
}