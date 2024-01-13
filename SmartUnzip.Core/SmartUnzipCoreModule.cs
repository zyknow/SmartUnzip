using Volo.Abp.Modularity;

namespace SmartUnzip.Core;

public class SmartUnzipCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
    }
}