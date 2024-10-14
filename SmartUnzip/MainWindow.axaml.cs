using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform;

namespace SmartUnzip;

public partial class MainWindow : UrsaWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Icon = new WindowIcon(
            AssetLoader.Open(new Uri(@$"avares://SmartUnzip/Assets/icon.ico")));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var topLevel = GetTopLevel(this);
        App.MainWindowNotification = new WindowNotificationManager(topLevel) { MaxItems = 5 };
    }
}