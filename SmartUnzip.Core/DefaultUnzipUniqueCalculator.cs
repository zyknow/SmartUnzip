using System.IO;

namespace SmartUnzip.Core;

public class DefaultUnzipUniqueCalculator : IUnzipUniqueCalculator
{
    public string GetUniqueFileName(string unzipFilePath, UnzipOptions options)
    {
        var fileName = Path.GetFileNameWithoutExtension(unzipFilePath);
        var extension = Path.GetExtension(unzipFilePath);
        var directory = Path.GetDirectoryName(unzipFilePath);
        var index = 1;
        var newFileName = unzipFilePath;
        while (File.Exists(newFileName))
        {
            newFileName = Path.Combine(directory, $"{fileName}({index}){extension}");
            index++;
        }

        return newFileName;
    }
}