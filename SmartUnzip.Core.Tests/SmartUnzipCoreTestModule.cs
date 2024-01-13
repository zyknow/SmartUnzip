using SmartUnzip.TestBase;
using Volo.Abp.Modularity;

namespace SmartUnzip.Core.Tests;

[DependsOn(typeof(SmartUnzipTestBaseModule), typeof(SmartUnzipCoreModule))]
public class SmartUnzipCoreTestModule : AbpModule
{
    
}