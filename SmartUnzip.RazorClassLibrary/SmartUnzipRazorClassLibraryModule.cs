using SmartUnzip.Core;
using SmartUnzip.Localization;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SmartUnzip;
[DependsOn(typeof(SmartUnzipLocalizationModule), typeof(SmartUnzipCoreModule), typeof(AbpAutofacModule))]
public class SmartUnzipRazorClassLibraryModule : AbpModule
{
}
