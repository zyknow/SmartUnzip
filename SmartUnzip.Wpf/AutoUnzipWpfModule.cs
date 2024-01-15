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

        services.AddFluentUIComponents();
        services.AddWpfBlazorWebView();
#if  DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var passwordRep = context.ServiceProvider.GetRequiredService<IPasswordRepository>();

        // TODO: 加载密码
        passwordRep.AddPasswords(new List<UnzipPassword>()
        {
            new UnzipPassword("123"),
            new UnzipPassword("456"),
        });

    }
}