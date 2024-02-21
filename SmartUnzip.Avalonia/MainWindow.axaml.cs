using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;
using SmartUnzip.Core;

namespace SmartUnzip.Avalonia;

public partial class MainWindow : Window
{
    public MainWindowViewModel vm { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        vm = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();

        DataContext = vm;

        Closed += (sender, args) => { App.ServiceProvider.GetRequiredService<IPasswordRepository>()?.Dispose(); };
    }
}