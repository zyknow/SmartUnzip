using Microsoft.Extensions.Logging;
using SmartUnzip.Unocss.Node;
using Volo.Abp.DependencyInjection;

namespace SmartUnzip.Unocss;
public class UnocssProcessInterop(ILogger<UnocssProcessInterop> logger) : IUnocssProcessInterop, ISingletonDependency
{
    NodeScriptRunner? nodeScriptRunner;
    public async Task StartProcessAsync(string scriptName, string workingDir)
    {
        await ExecuteScript(workingDir, scriptName, logger);
    }



    private async Task ExecuteScript(
      string sourcePath,
      string scriptName,
      ILogger logger)
    {
        nodeScriptRunner = new NodeScriptRunner(sourcePath, scriptName, null,new Dictionary<string, string>(), "npm", logger, new CancellationToken());
        //nodeScriptRunner.AttachToLogger(logger, true);
        //using EventedStreamStringReader stdErrReader = new(nodeScriptRunner.StdErr);
        //try
        //{
        //    Match match = await nodeScriptRunner.StdOut.WaitForMatch(new Regex("UnoCSS in watch mode...", RegexOptions.None));
        //}
        //catch (EndOfStreamException ex)
        //{
        //    throw new InvalidOperationException("The npm script '" + scriptName + "' exited without indicating that the Tailwind CLI had finished. The error output was: " + stdErrReader.ReadAsString(), (Exception)ex);
        //}
    }

    public void Dispose()
    {
        nodeScriptRunner?.Dispose();
    }
}
