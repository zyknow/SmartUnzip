using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using SmartUnzip.Unocss;
using Volo.Abp;

namespace SmartUnzip;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IAbpApplicationWithInternalServiceProvider? _abpApplication;

    protected override async void OnStartup(StartupEventArgs e)
    {

        Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.File("Logs/logs.txt"))
    .CreateLogger();

        try
        {
            Log.Information("Starting WPF host.");

            _abpApplication = await AbpApplicationFactory.CreateAsync<AutoUnzipWpfModule>(options =>
            {
                options.UseAutofac();
                options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            });

            await _abpApplication.InitializeAsync();

            _abpApplication.Services.GetRequiredService<MainWindow>()?.Show();

        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
        }

        Resources.Add("services", _abpApplication!.ServiceProvider);
        var unocss = _abpApplication.ServiceProvider.GetRequiredService<IUnocssProcessInterop>();
#if DEBUG
        // unocss?.StartProcessAsync("watch", "../");
#endif

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var unocss = _abpApplication?.ServiceProvider.GetRequiredService<IUnocssProcessInterop>();
        unocss?.Dispose();

        base.OnExit(e);
    }
}

