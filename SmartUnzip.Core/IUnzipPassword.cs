namespace SmartUnzip.Core;

public interface IUnzipPassword
{
    public string Value { get; set; }

    public uint UseCount { get; set; }

    public uint ManualSort { get; set; }

    void IncreaseUseCount();
}