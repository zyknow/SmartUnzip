using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using SmartUnzip.Core;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SmartUnzip;

[DependsOn(typeof(AbpAutofacModule), typeof(SmartUnzipCoreModule))]
public class AutoUnzipWpfModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddFluentUIComponents();
        services.AddWpfBlazorWebView();
    }

    public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        return base.OnApplicationInitializationAsync(context);
    }

    public override Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
    {
        return base.OnApplicationShutdownAsync(context);
    }
}