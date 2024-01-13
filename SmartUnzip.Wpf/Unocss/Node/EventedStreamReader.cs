using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartUnzip.Unocss.Node;
internal class EventedStreamReader
{
    private readonly StreamReader _streamReader;
    private readonly StringBuilder _linesBuffer;

    public event OnReceivedChunkHandler? OnReceivedChunk;

    public event OnReceivedLineHandler? OnReceivedLine;

    public event OnStreamClosedHandler? OnStreamClosed;

    public EventedStreamReader(StreamReader streamReader)
    {
        _streamReader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
        _linesBuffer = new StringBuilder();
        Task.Factory.StartNew(new Func<Task>(Run), CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    public Task<Match> WaitForMatch(Regex regex)
    {
        TaskCompletionSource<Match> tcs = new();
        object completionLock = new();
        OnReceivedLineHandler onReceivedLineHandler = null;
        OnStreamClosedHandler onStreamClosedHandler = null;
        onReceivedLineHandler = line =>
        {
            Match match = regex.Match(line);
            if (!match.Success)
                return;
            ResolveIfStillPending(() => tcs.SetResult(match));
        };
        onStreamClosedHandler = () => ResolveIfStillPending(() => tcs.SetException(new EndOfStreamException()));
        OnReceivedLine += onReceivedLineHandler;
        OnStreamClosed += onStreamClosedHandler;
        return tcs.Task;

        void ResolveIfStillPending(Action applyResolution)
        {
            lock (completionLock)
            {
                if (tcs.Task.IsCompleted)
                    return;
                OnReceivedLine -= onReceivedLineHandler;
                OnStreamClosed -= onStreamClosedHandler;
                applyResolution();
            }
        }
    }

    private async Task Run()
    {
        char[] buf = new char[8192];
        while (true)
        {
            int count = 0;
            int startIndex = 0;
            int num = 0;
            do
            {
                count = await _streamReader.ReadAsync(buf, 0, buf.Length);
                if (count == 0)
                {
                    if (_linesBuffer.Length > 0)
                    {
                        OnCompleteLine(_linesBuffer.ToString());
                        _linesBuffer.Clear();
                    }
                    OnClosed();
                    buf = null;
                }
                else
                {
                    OnChunk(new ArraySegment<char>(buf, 0, count));
                    for (startIndex = 0; (num = Array.IndexOf(buf, '\n', startIndex, count - startIndex)) >= 0 && startIndex < count; startIndex = num + 1)
                    {
                        int charCount = num + 1 - startIndex;
                        _linesBuffer.Append(buf, startIndex, charCount);
                        OnCompleteLine(_linesBuffer.ToString());
                        _linesBuffer.Clear();
                    }
                }
            }
            while (num >= 0 || startIndex >= count);
            _linesBuffer.Append(buf, startIndex, count - startIndex);
        }
    }

    private void OnChunk(ArraySegment<char> chunk)
    {
        OnReceivedChunkHandler onReceivedChunk = OnReceivedChunk;
        if (onReceivedChunk == null)
            return;
        onReceivedChunk(chunk);
    }

    private void OnCompleteLine(string line)
    {
        OnReceivedLineHandler onReceivedLine = OnReceivedLine;
        if (onReceivedLine == null)
            return;
        onReceivedLine(line);
    }

    private void OnClosed()
    {
        OnStreamClosedHandler onStreamClosed = OnStreamClosed;
        if (onStreamClosed == null)
            return;
        onStreamClosed();
    }

    public delegate void OnReceivedChunkHandler(ArraySegment<char> chunk);

    public delegate void OnReceivedLineHandler(string line);

    public delegate void OnStreamClosedHandler();
}