namespace SmartUnzip.Core;

public interface IUnzipUniqueCalculator
{
    public string GetUniqueFileName(string unzipFilePath, UnzipOptions options);
}