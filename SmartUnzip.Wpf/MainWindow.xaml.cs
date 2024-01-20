using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
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

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        var data = e.Data.GetData(DataFormats.FileDrop);
    }
}