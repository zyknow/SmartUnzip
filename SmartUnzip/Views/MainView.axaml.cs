using Avalonia.Controls;
using Avalonia.Interactivity;
using SmartUnzip.ViewModels;

namespace SmartUnzip.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        DataContext = new MainViewModel();
        
        
        
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Menu.SelectedItem = Menu.Items[0];
    }

    private void ChangeMenuCollapsed(object? sender, RoutedEventArgs e)
    {
        Menu.IsHorizontalCollapsed = !Menu.IsHorizontalCollapsed;
    }
}