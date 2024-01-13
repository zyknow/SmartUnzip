using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SmartUnzip.TestBase;

public class SmartUnzipTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public Task SeedAsync(DataSeedContext context)
    {
        /* Seed additional test data... */

        return Task.CompletedTask;
    }
}