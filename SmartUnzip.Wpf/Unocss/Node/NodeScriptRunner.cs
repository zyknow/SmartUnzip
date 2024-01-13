using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SmartUnzip.Unocss.Node;
internal class NodeScriptRunner : IDisposable
{
    private Process? _npmProcess;
    private static readonly Regex AnsiColorRegex = new("\u001B\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1.0));

    public EventedStreamReader StdOut { get; }

    public EventedStreamReader StdErr { get; }

    public NodeScriptRunner(
      string workingDirectory,
      string scriptName,
      string? arguments,
      IDictionary<string, string>? envVars,
      string pkgManagerCommand,
      ILogger logger,
      CancellationToken applicationStoppingToken)
    {
        if (string.IsNullOrEmpty(workingDirectory))
            throw new ArgumentException("Cannot be null or empty.", nameof(workingDirectory));
        if (string.IsNullOrEmpty(scriptName))
            throw new ArgumentException("Cannot be null or empty.", nameof(scriptName));
        string fileName = !string.IsNullOrEmpty(pkgManagerCommand) ? pkgManagerCommand : throw new ArgumentException("Cannot be null or empty.", nameof(pkgManagerCommand));
        string str = "run " + scriptName + " -- " + (arguments ?? string.Empty);
        if (OperatingSystem.IsWindows())
        {
            fileName = "cmd";
            str = "/c " + pkgManagerCommand + " " + str;
        }
        ProcessStartInfo startInfo = new(fileName)
        {
            Arguments = str,
            UseShellExecute = false,
            //RedirectStandardInput = true,
            //RedirectStandardOutput = true,
            //RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };
        if (envVars != null)
        {
            foreach (KeyValuePair<string, string> envVar in (IEnumerable<KeyValuePair<string, string>>)envVars)
                startInfo.Environment[envVar.Key] = envVar.Value;
        }
        _npmProcess = LaunchNodeProcess(startInfo, pkgManagerCommand);
        //StdOut = new EventedStreamReader(_npmProcess.StandardOutput);
        //StdErr = new EventedStreamReader(_npmProcess.StandardError);
        //applicationStoppingToken.Register(new Action(((IDisposable)this).Dispose));

        //_npmProcess.StartInfo = startInfo;

        //Task.Run(_npmProcess.Start);

        //logger.LogInformation("NPM process started", new
        //{
        //    processStartInfo = startInfo,
        //    process = _npmProcess
        //});
    }

    //public void AttachToLogger(ILogger logger, bool treatErrorsAsInfo = false)
    //{
    //    StdOut.OnReceivedLine += line =>
    //    {
    //        if (string.IsNullOrWhiteSpace(line))
    //            return;
    //        logger.LogInformation(StripAnsiColors(line));
    //    };
    //    StdErr.OnReceivedLine += line =>
    //    {
    //        if (string.IsNullOrWhiteSpace(line))
    //            return;
    //        if (treatErrorsAsInfo)
    //            logger.LogInformation(StripAnsiColors(line));
    //        else
    //            logger.LogError(StripAnsiColors(line));
    //    };
    //    StdErr.OnReceivedChunk += chunk =>
    //    {
    //        if (Array.IndexOf(chunk.Array, '\n', chunk.Offset, chunk.Count) >= 0)
    //            return;
    //        Console.Write(chunk.Array, chunk.Offset, chunk.Count);
    //    };
    //}

    //private static string StripAnsiColors(string line)
    //{
    //    return AnsiColorRegex.Replace(line, string.Empty);
    //}

    private static Process LaunchNodeProcess(ProcessStartInfo startInfo, string commandName)
    {
        try
        {
            Process process = Process.Start(startInfo);
            process.EnableRaisingEvents = true;
            return process;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to start '" + commandName + "'. To resolve this:.\n\n[1] Ensure that '" + commandName + "' is installed and can be found in one of the PATH directories.\n    Current PATH enviroment variable is: " + Environment.GetEnvironmentVariable("PATH") + "\n    Make sure the executable is in one of those directories, or update your PATH.\n\n[2] See the InnerException for further details of the cause.", ex);
        }
    }

    public void Dispose()
    {
        if (_npmProcess == null || _npmProcess.HasExited)
            return;
        _npmProcess.Kill(true);
        _npmProcess = null;
    }
}