using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SmartUnzip.Components;

public partial class TitleBarRightContent : UserControl
{
    private PasswordWindow _passwordWindow = new();
    
    public TitleBarRightContent()
    {
        InitializeComponent();
    }

    private async void OpenRepository(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri("https://github.com/irihitech/Ursa.Avalonia"));
    }

    private void ShowPasswordInputWindowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _passwordWindow.Show();
    }
}