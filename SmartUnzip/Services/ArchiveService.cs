using System.Diagnostics;
using System.Threading;
using SmartUnzip.Core;

namespace SmartUnzip.Services;

public class ArchiveService
{
    protected string SevenZPath => App.Settings.SevenZFilePath;

    public async Task ExtractAsync(string archivePath, string? outputPath,
        string? password = null,
        bool notKeepDirectoryStructure = false,
        DuplicateFileHandleMode duplicateFileHandleMode = DuplicateFileHandleMode.Skip,
        CancellationToken cancellationToken = default)
    {
        using (Process process = new Process())
        {
            var duplicateFileHandleStr = duplicateFileHandleMode switch
            {
                DuplicateFileHandleMode.Skip => "-aos",
                DuplicateFileHandleMode.Overwrite => "-aoa",
                DuplicateFileHandleMode.Rename => "-aou",
                _ => ""
            };

            process.StartInfo.FileName = SevenZPath; // 7z.exe的路径
            process.StartInfo.Arguments =
                $"{(notKeepDirectoryStructure ? "e" : "x")} \"{archivePath}\" {(string.IsNullOrWhiteSpace(outputPath) ? "" : @$"-o{outputPath}")} {duplicateFileHandleStr} {(!string.IsNullOrEmpty(password) ? @$"-p{password}" : "")} -y"; // 解压命令
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // 隐藏命令行窗口
            process.StartInfo.RedirectStandardOutput = true; // 重定向输出，以便读取
            process.StartInfo.RedirectStandardError = true; // 重定向错误输出，以便读取
            process.StartInfo.UseShellExecute = false; // 需要重定向输出时必须设置为false
            process.Start();


            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                // 读取输出，支持CancellationToken
                string output = await process.StandardOutput.ReadToEndAsync(cancellationToken)
                    .WaitAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken); // 等待命令执行完成，支持CancellationToken

                if (output.Contains("Everything is Ok"))
                {
                    return;
                }
                else
                {
                    throw new Exception("解压失败");
                }
            }
        }
    }

    public async Task<bool> TestArchivePasswordAsync(string archivePath, string? password = null,
        CancellationToken cancellationToken = default)
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = SevenZPath; // 7z.exe的路径
            process.StartInfo.Arguments = $"t \"{archivePath}\" -p{password}"; // 测试命令
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // 隐藏命令行窗口
            process.StartInfo.RedirectStandardOutput = true; // 重定向输出，以便读取
            process.StartInfo.RedirectStandardError = true; // 重定向错误输出，以便读取
            process.StartInfo.UseShellExecute = false; // 需要重定向输出时必须设置为false

            process.Start();

            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                // 读取输出，支持CancellationToken
                string output = await process.StandardOutput.ReadToEndAsync(cancellationToken)
                    .WaitAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken); // 等待命令执行完成，支持CancellationToken

                return output.Contains("Everything is Ok"); // 如果输出包含"Everything is Ok"，则密码正确
            }
        }
    }

    public async Task<ArchiveFileValidResult> TestArchiveAsync(string archivePath,
        CancellationToken cancellationToken = default)
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = SevenZPath; // 7z.exe的路径
            process.StartInfo.Arguments = $"t \"{archivePath}\" -p"; // 测试命令
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // 隐藏命令行窗口
            process.StartInfo.RedirectStandardOutput = true; // 重定向输出，以便读取
            process.StartInfo.RedirectStandardError = true; // 重定向错误输出，以便读取

            process.StartInfo.UseShellExecute = false; // 需要重定向输出时必须设置为false

            process.Start();

            using (cancellationToken.Register(() => process.Kill())) // 如果取消操作发生，杀掉进程
            {
                // 读取输出，支持CancellationToken
                string output = await process.StandardOutput.ReadToEndAsync(cancellationToken)
                    .WaitAsync(cancellationToken);

                output += await process.StandardError.ReadToEndAsync(cancellationToken)
                    .WaitAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken); // 等待命令执行完成，支持CancellationToken

                return new()
                {
                    FilePath = archivePath,
                    ServenZOuput = output,
                    IsValid = (!output.Contains("Cannot open the file as archive") &&
                               !output.Contains("Can't open as archive")) ||
                              output.Contains("Cannot open encrypted archive."),
                    IsVolume = output.Contains("Volumes =")
                };
            }
        }
    }
}