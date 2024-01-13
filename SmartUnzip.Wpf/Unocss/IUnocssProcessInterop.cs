namespace SmartUnzip.Unocss;
public interface IUnocssProcessInterop : IDisposable
{
    Task StartProcessAsync(string scriptName, string workingDir);
}
