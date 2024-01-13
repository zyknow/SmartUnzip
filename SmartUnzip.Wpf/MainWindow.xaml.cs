using System.Windows;
using Volo.Abp.DependencyInjection;

namespace SmartUnzip;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, ITransientDependency
{
    public MainWindow()
    {
        InitializeComponent();
    }
}