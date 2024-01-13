using System.Text;

namespace SmartUnzip.Unocss.Node;
internal class EventedStreamStringReader : IDisposable
{
    private readonly EventedStreamReader _eventedStreamReader;
    private bool _isDisposed;
    private readonly StringBuilder _stringBuilder = new();

    public EventedStreamStringReader(EventedStreamReader eventedStreamReader)
    {
        _eventedStreamReader = eventedStreamReader ?? throw new ArgumentNullException(nameof(eventedStreamReader));
        _eventedStreamReader.OnReceivedLine += new EventedStreamReader.OnReceivedLineHandler(OnReceivedLine);
    }

    public string ReadAsString() => _stringBuilder.ToString();

    private void OnReceivedLine(string line) => _stringBuilder.AppendLine(line);

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _eventedStreamReader.OnReceivedLine -= new EventedStreamReader.OnReceivedLineHandler(OnReceivedLine);
        _isDisposed = true;
    }
}
