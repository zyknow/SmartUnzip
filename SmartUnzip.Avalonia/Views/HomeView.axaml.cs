using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;

namespace SmartUnzip.Avalonia.Views;

public partial class HomeView : UserControl
{
    public HomeViewModel vm { get; set; }

    public HomeView()
    {
        InitializeComponent();
        vm = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        DataContext = vm;
        // unzipPackageAfterHandleTypeComBox.SelectedItem = UnzipPackageAfterHandleTypeEnumNames[0];
        GridDragDrop.AddHandler(DragDrop.DropEvent, Grid_Drop);
        DragDrop.SetAllowDrop(this, true);
    }

    private async Task Grid_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var filePaths = e.Data.GetFileNames();
            await vm.OnDrop(filePaths);

        }
    }
}