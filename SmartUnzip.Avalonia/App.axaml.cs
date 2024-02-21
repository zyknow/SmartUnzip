using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartUnzip.Avalonia.ViewModels;
using SmartUnzip.Core;

namespace SmartUnzip.Avalonia;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();

        services.AddTransient<HomeViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PasswordManagerViewModel>();

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        services.AddSmartUnzipServices(opt =>
        {
            opt.ArchiveFileInfoDefineType = typeof(ArchiveFileInfo);
            opt.UnzipPasswordDefineType = typeof(UnzipPassword);
        });

        ServiceProvider = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}