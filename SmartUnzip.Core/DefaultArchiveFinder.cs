using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartUnzip.Core;

public class DefaultArchiveFinder(
    IArchiveProcessor archiveProcessor,
    ILogger<DefaultArchiveFinder> logger,
    IOptions<UnzipOptions> unzipOptions)
    : IArchiveFinder, ITransientDependency
{
    public virtual Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(string directory, bool recursive = false)
    {
        return FindArchiveAsync(new DirectoryInfo(directory), recursive);
    }

    public virtual async Task<IEnumerable<ArchiveFileInfo>> FindArchiveAsync(DirectoryInfo directory,
        bool recursive = false)
    {
        List<ArchiveFileInfo> infos = [];

        var files = directory.GetFiles();

        if (files.IsNullOrEmpty())
            return [];

        foreach (var fileInfo in files)
        {
            // 排除已经处理过的文件
            if (infos.Any(info => info.Parts.Contains(fileInfo.FullName)))
                continue;

            try
            {
                var info = archiveProcessor.GetArchiveFileInfo(fileInfo.FullName, unzipOptions.Value.ExcludeRegexs,
                    unzipOptions.Value.SupportArchiveTypes);
                infos.Add(info);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        if (!recursive) return infos;

        var subDirectories = directory.GetDirectories();
        foreach (var subDirectory in subDirectories)
        {
            infos.AddRange(await FindArchiveAsync(subDirectory, true));
        }

        return infos;
    }
}