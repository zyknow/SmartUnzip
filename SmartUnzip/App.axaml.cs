using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;
using SmartUnzip.Core;

namespace SmartUnzip;

public partial class App : Application
{
    public static SettingsOptions Settings { get; set; }

    public static SemaphoreSlim UnzipSemaphore { get; set; }
    public static SemaphoreSlim PasswordTestSemaphore { get; set; }

    public static Window MainWindow { get; private set; }

    public static WindowNotificationManager MainWindowNotification;

    public App()
    {
        Settings = new SettingsOptions();
        Settings.LoadSettings();

        UnzipSemaphore = new SemaphoreSlim(Settings.MaxUnzipThreadCount);
        PasswordTestSemaphore = new SemaphoreSlim(Settings.MaxTestPasswordThreadCount);

        Settings.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SettingsOptions.MaxUnzipThreadCount))
            {
                UnzipSemaphore = new SemaphoreSlim(Settings.MaxUnzipThreadCount);
            }

            if (args.PropertyName == nameof(SettingsOptions.MaxTestPasswordThreadCount))
            {
                PasswordTestSemaphore = new SemaphoreSlim(Settings.MaxTestPasswordThreadCount);
            }
        };

        Dispatcher.UIThread.UnhandledException += UIThreadOnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }
    
    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
  
    }

    private void UIThreadOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var exp = e.Exception;

        Log.Error(exp, "Exception");

        try
        {
            App.MainWindowNotification?.Show(new Notification("异常", exp.Message,
                NotificationType.Error));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }


        e.Handled = true;
    }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 如果使用 CommunityToolkit，则需要用下面一行移除 Avalonia 数据验证。
        // 如果没有这一行，数据验证将会在 Avalonia 和 CommunityToolkit 中重复。
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}