using Microsoft.JSInterop;
using Serilog.Core;
using Serilog.Events;

public class JSConsoleSink : ILogEventSink
{
    private readonly IJSRuntime _jsRuntime;

    public JSConsoleSink(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null) return;

        var message = logEvent.RenderMessage();
        // 根据日志级别，调用相应的浏览器控制台函数
        switch (logEvent.Level)
        {
            case LogEventLevel.Error:
                _jsRuntime.InvokeVoidAsync("console.error", message);
                break;
            case LogEventLevel.Warning:
                _jsRuntime.InvokeVoidAsync("console.warn", message);
                break;
            case LogEventLevel.Information:
                _jsRuntime.InvokeVoidAsync("console.info", message);
                break;
            default:
                _jsRuntime.InvokeVoidAsync("console.log", message);
                break;
        }
    }
}