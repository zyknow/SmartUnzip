namespace SmartUnzip.Core;

public interface IUnzipUniqueCalculator
{
    public string GetUniqueFileName(string unzipFilePath, IUnzipOptions options);
}