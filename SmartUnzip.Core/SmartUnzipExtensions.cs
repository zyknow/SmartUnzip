using Microsoft.Extensions.DependencyInjection;

namespace SmartUnzip.Core;

public static class SmartUnzipExtensions
{
    public static void AddSmartUnzipServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordRepository, DefaultPasswordRepository>();
        services.AddTransient<IUnzipExtractor, DefaultUnzipExtractor>();
        services.AddTransient<IUnzipUniqueCalculator, DefaultUnzipUniqueCalculator>();
    }
}