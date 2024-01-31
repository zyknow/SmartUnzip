using System;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace SmartUnzip.Core.Tests.Core;

public class UnzipTests
{
    private string filePath = @$"C:\Users\zy\Desktop\temp\test\TestZips - 副本\Parts-Password\7z-password-123.7z.001";

    [Fact]
    public void UnzipDeleteTest()
    {
        var filePaths = ArchiveFactory.GetFileParts(filePath).ToList();

        var fileInfos = filePaths.Select(x => new FileInfo(x));

        var readerOptions = new ReaderOptions();
        
        // readerOptions.LeaveStreamOpen = true;

        IArchive archive = null;
        
        
        try
        {
            archive = ArchiveFactory.Open(fileInfos);
        }
        catch (CryptographicException e)
        {
        }
        
        readerOptions.Password = "123";
        var archive1 
            = ArchiveFactory.Open(fileInfos, readerOptions);


        var exPath = $@"C:\Users\zy\Desktop\temp\test\TestZips - 副本\Parts-Password\test";

        DirectoryHelper.CreateIfNotExists(exPath);

        archive1.WriteToDirectory(exPath, new ExtractionOptions()
        {
            ExtractFullPath = true
        });

        archive1.Dispose();
        Console.WriteLine();
    }
}