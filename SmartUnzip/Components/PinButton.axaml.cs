using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SmartUnzip.Components;

public partial class PinButton : UserControl
{
    public PinButton()
    {
        InitializeComponent();
    }
    
    private void PinButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.VisualRoot is Window window)
        {
            window.Topmost = !window.Topmost;
        }
    }
}