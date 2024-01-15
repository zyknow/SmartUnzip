using SmartUnzip.Localization.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace SmartUnzip.Localization;

[DependsOn(
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule)
    )]
public class SmartUnzipLocalizationModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
  
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SmartUnzipLocalizationModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SmartUnzipResource>("zh")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/SmartUnzip");

            options.DefaultResourceType = typeof(SmartUnzipResource);
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SmartUnzip", typeof(SmartUnzipResource));
        });
    }
}
