using System.Diagnostics;
using System.IO;
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

    private Process? process;


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
#if DEBUG
        //StartUnocss("../");
#endif

        base.OnStartup(e);
    }

    public void StartUnocss(string relativePath)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var projectDirectory = baseDirectory;

        while (projectDirectory != null)
        {
            var csprojFile = Directory.EnumerateFiles(projectDirectory, "*.csproj").FirstOrDefault();
            if (csprojFile != null)
            {
                break;
            }

            projectDirectory = Directory.GetParent(projectDirectory)?.FullName;
        }

        if (projectDirectory == null)
        {
            throw new DirectoryNotFoundException("Could not find a directory containing a .csproj file.");
        }

        var workingDirectory = Path.GetFullPath(Path.Combine(projectDirectory, relativePath));

        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                WorkingDirectory = workingDirectory,
                Arguments = "npm run watch",
                UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                CreateNoWindow = false,
            }
        };

        //process.OutputDataReceived += (sender, args) => 
        //    Console.WriteLine(args.Data);
        //process.ErrorDataReceived += (sender, args) => 
        //    Console.Error.WriteLine(args.Data);

        process.Start();

        //process.BeginOutputReadLine();
        //process.BeginErrorReadLine();

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (process is { HasExited: false })
            {
                process.Kill();
            }
        };
    }


    protected override void OnExit(ExitEventArgs e)
    {
        var unocss = _abpApplication?.ServiceProvider.GetRequiredService<IUnocssProcessInterop>();
        unocss?.Dispose();

        base.OnExit(e);
    }
}

