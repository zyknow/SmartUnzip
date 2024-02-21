using System;
using Microsoft.Extensions.DependencyInjection;

namespace SmartUnzip.Core;

public static class SmartUnzipExtensions
{
    public static void AddSmartUnzipServices(this IServiceCollection services,
        Action<SmartUnzipOptions> smartUnzipOptionsAction)
    {
        var options = new SmartUnzipOptions();

        // smartUnzipOptionsAction?.Invoke(options);
        
        services.AddSingleton(options);
        
        // services.Configure<SmartUnzipOptions>(opts =>
        // {
        //     smartUnzipOptionsAction?.Invoke(opts);
        // });

        services.Configure(smartUnzipOptionsAction);

        services.AddSingleton<IPasswordRepository, DefaultPasswordRepository>();
        services.AddTransient<IUnzipExtractor, DefaultUnzipExtractor>();
        services.AddTransient<IUnzipUniqueCalculator, DefaultUnzipUniqueCalculator>();
    }
}