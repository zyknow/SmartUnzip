using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Bing.Collections;
using Bing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;
using SmartUnzip.Core;

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


    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        vm.StateChanged();
    }
}