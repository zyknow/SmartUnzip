using SmartUnzip.Blazor.Components;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SmartUnzip.Blazor;

[DependsOn(typeof(SmartUnzipRazorClassLibraryModule), typeof(AbpAutofacModule), typeof(AbpAspNetCoreModule))]
public class SmartUnzipBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddRazorComponents()
            .AddInteractiveServerComponents();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();
        
        
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();
        
        var webApp = app as WebApplication;
        
        webApp.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
    }
}