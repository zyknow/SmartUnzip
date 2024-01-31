using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;

namespace SmartUnzip.Avalonia;

public partial class MainWindow : Window
{
    public MainWindowViewModel vm { get; set; }
    public MainWindow()
    {
        InitializeComponent();
        
        vm = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();

        DataContext = vm;
    }

}