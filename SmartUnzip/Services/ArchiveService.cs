using System.Diagnostics;
using System.Threading;
using Serilog;
using SmartUnzip.Core;

namespace SmartUnzip.Services;

public class ArchiveService
{
    protected string SevenZPath => App.Settings.SevenZFilePath;

    public async Task ExtractAsync(string archivePath, string? outputPath,
        string? password = null,
        DuplicateFileHandleMode duplicateFileHandleMode = DuplicateFileHandleMode.Skip,
        CancellationToken cancellationToken = default)
    {
        var duplicateFileHandleStr = duplicateFileHandleMode switch
        {
            DuplicateFileHandleMode.Skip => "-aos",
            DuplicateFileHandleMode.Overwrite => "-aoa",
            DuplicateFileHandleMode.Rename => "-aou",
            _ => ""
        };

        var command =
            $"x \"{archivePath}\" {(string.IsNullOrWhiteSpace(outputPath) ? "" : @$"-o{$"\"{outputPath}\""}")} {duplicateFileHandleStr} {(!string.IsNullOrEmpty(password) ? @$"-p{password}" : "")} -y";

        using (Process process = CreateProcess(command))
        {
            process.Start();


            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                var (output, errorOutput) = await ReadOutputAsync(process, cancellationToken);

                // await process.WaitForExitAsync(cancellationToken); 

                if (!string.IsNullOrWhiteSpace(errorOutput))
                {
                    Log.Error(
                        $"解压{archivePath} => {outputPath} 失败: \r\noutput: \r\n {output}   \r\nerror : \r\n {errorOutput}");
                    throw new Exception("解压失败");
                }
            }
        }
    }

    public async Task<bool> TestArchivePasswordAsync(string archivePath, string? password = null,
        CancellationToken cancellationToken = default)
    {
        var command = $"t \"{archivePath}\" -p{password}";

        using (Process process = CreateProcess(command, false))
        {
            process.Start();

            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                var (output, errorOutput) = await ReadOutputAsync(process, cancellationToken);

                // await process.WaitForExitAsync(cancellationToken); 

                var succeeded =
                    string.IsNullOrWhiteSpace(errorOutput) &&
                    output.Contains("Everything is Ok") &&
                    !output.Contains("No files to process"); // 如果输出包含"Everything is Ok"，则密码正确

                return succeeded;
            }
        }
    }

    public async Task<ArchiveFileValidResult> TestArchiveAsync(string archivePath,
        CancellationToken cancellationToken = default)
    {
        var command = $"t \"{archivePath}\" -p";

        using (Process process = CreateProcess(command))
        {
            process.Start();

            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                var (output, errorOutput) = await ReadOutputAsync(process, cancellationToken);
                output += errorOutput;

                // await process.WaitForExitAsync(cancellationToken);

                ArchiveFileValidResult result = GetArchiveFileValidResult(output, archivePath);

                return result;
            }
        }
    }

    public async Task<bool> CheckIsSingleFileOrFolderAsync(string archivePath, string? password = null,
        CancellationToken cancellationToken = default)
    {
        using (Process process = CreateProcess($"l \"{archivePath}\" -p{password}"))
        {
            process.Start();

            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                var (output, errorOutput) = await ReadOutputAsync(process, cancellationToken);

                // await process.WaitForExitAsync(cancellationToken); 

                // var lines = output.Split("\r").ToList();
                // var skipIndex = lines.FindIndex(x => x.Contains("------------------------"));
                // lines = lines.Skip(skipIndex).ToList();
                // var content = lines.Last();

                var index = output.LastIndexOf("------------------------", StringComparison.Ordinal);
                var content = output.Substring(index, output.Length - index).Split("\r")[1];

                var isSingleFile = content.Contains("1 files");
                var isSingleFolder = content.Contains("1 folders");

                return isSingleFile ^ isSingleFolder;
            }
        }
    }


    public async Task<List<ArchiveFileValidResult>> SearchArchiveFilesAsync(string dirPath,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(dirPath))
            return [];

        var command = $"t \"{dirPath}\" -p";

        using (Process process = CreateProcess(command))
        {
            process.Start();

            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                var (output, errorOutput) = await ReadOutputAsync(process, cancellationToken);

                // await process.WaitForExitAsync(cancellationToken); 

                var archiveInfos = output.Split("Testing archive: ").Skip(1).ToList();

                var archiveErrorInfos = errorOutput.Split("ERROR: ").ToList();

                var archiveResults = archiveInfos.Select(x =>
                {
                    var path = x.Split("\r")[0];
                    var fullOutput = $"{x}{archiveErrorInfos.FirstOrDefault(e => e.StartsWith($"{path}\r"))}";

                    return GetArchiveFileValidResult(fullOutput, path);
                }).Where(x => x.IsValid).ToList();

                return archiveResults;
            }
        }
    }

    ArchiveFileValidResult GetArchiveFileValidResult(string fullOutput, string archivePath)
    {
        ArchiveFileValidResult result = new()
        {
            FilePath = archivePath,
            ServenZOuput = fullOutput,
            IsValid = (
                          !fullOutput.Contains("Type = PE") &&
                          !fullOutput.Contains("Characteristics = Executable DLL") &&
                          !fullOutput.Contains("Cannot open the file as archive") &&
                          !fullOutput.Contains("Can't open as archive")) ||
                      fullOutput.Contains("Cannot open encrypted archive."),
            IsVolume = fullOutput.Contains("Volume Index = 0")
        };

        return result;
    }


    private Process CreateProcess(string command, bool logCommand = true)
    {
        Process process = new();
        process.StartInfo.FileName = SevenZPath; // 7z.exe的路径
        process.StartInfo.Arguments = command;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // 隐藏命令行窗口
        process.StartInfo.RedirectStandardOutput = true; // 重定向输出，以便读取
        process.StartInfo.RedirectStandardError = true; // 重定向错误输出，以便读取
        process.StartInfo.UseShellExecute = false; // 需要重定向输出时必须设置为false

        if (logCommand)
        {
            Log.Information($"7z command: {command}");
        }

        return process;
    }

    private async Task<(string output, string ErrorOutput)> ReadOutputAsync(Process process,
        CancellationToken cancellationToken)
    {
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var res = await Task.WhenAll(outputTask, errorTask);
        return (res[0], res[1]);
    }
}