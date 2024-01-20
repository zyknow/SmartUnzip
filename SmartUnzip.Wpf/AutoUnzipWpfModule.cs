using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using SmartUnzip.Core;
using SmartUnzip.Core.Models;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace SmartUnzip;

[DependsOn(typeof(SmartUnzipRazorClassLibraryModule))]
public class AutoUnzipWpfModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddWpfBlazorWebView();
#if  DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }

}