using Tailwind;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Components.Server;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Modularity;

namespace SmartUnzip.Blazor;

[DependsOn(typeof(SmartUnzipRazorClassLibraryModule), typeof(AbpAspNetCoreComponentsServerModule))]
public class SmartUnzipBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.AddRazorPages();
        services.AddServerSideBlazor();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder() as WebApplication;


        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        if (env.IsDevelopment())
        {
            app.RunTailwind("watch:blazor-server", "../");
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();

        //app.MapRazorPages();
        app.MapFallbackToPage("/_Host");
    }
}
