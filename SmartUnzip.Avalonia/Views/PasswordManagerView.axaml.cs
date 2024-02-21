using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Bing.Extensions;
using Bing.Text;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;
using SmartUnzip.Core;
using Ursa.Controls;

namespace SmartUnzip.Avalonia.Views;

public partial class PasswordManagerView : UserControl
{
    public PasswordManagerViewModel vm { get; set; }


    public PasswordManagerView()
    {
        InitializeComponent();
        vm = App.ServiceProvider.GetRequiredService<PasswordManagerViewModel>();


        var dataGridSortDescription = DataGridSortDescription.FromPath(nameof(UnzipPassword.Value),
            ListSortDirection.Ascending, new ReversedStringComparer());
        var collectionView1 = new DataGridCollectionView(vm.Passwords);
        collectionView1.SortDescriptions.Add(dataGridSortDescription);
        var dg1 = this.Get<DataGrid>("dataGrid1");
        dg1.IsReadOnly = true;
        dg1.LoadingRow += Dg1_LoadingRow;
        dg1.Sorting += (s, a) =>
        {
            var binding = (a.Column as DataGridBoundColumn)?.Binding as Binding;

            if (binding?.Path is string property
                && property == dataGridSortDescription.PropertyPath
                && !collectionView1.SortDescriptions.Contains(dataGridSortDescription))
            {
                collectionView1.SortDescriptions.Add(dataGridSortDescription);
            }
        };
        dg1.ItemsSource = collectionView1;
        DataContext = vm;


        GridDragDrop.AddHandler(DragDrop.DropEvent, Border_Drop);
        DragDrop.SetAllowDrop(this, true);
    }

    private async Task Border_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var filePaths = e.Data.GetFileNames();

            var passwordRep = App.ServiceProvider.GetRequiredService<IPasswordRepository>();
            try
            {
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        if (Path.GetExtension(filePath) == ".txt")
                        {
                            var passwordText = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                            foreach (string line in passwordText.Split("\r\n"))
                            {
                                var password = line.Split("\t").First().Trim();
                                if (!password.IsNullOrWhiteSpace())
                                    passwordRep.AddPassword(new UnzipPassword(password));
                            }
                        }
                        else if (Path.GetExtension(filePath) == ".json")
                        {
                            passwordRep.LoadPassword(filePath);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                await MessageBox.ShowAsync(exception.Message, "异常");
            }

            vm.LoadPasswords();
        }
    }


    private void Dg1_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Header = e.Row.GetIndex() + 1;
    }

    private class ReversedStringComparer : IComparer<object>, IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is string left && y is string right)
            {
                var reversedLeft = new string(left.Reverse().ToArray());
                var reversedRight = new string(right.Reverse().ToArray());
                return reversedLeft.CompareTo(reversedRight);
            }

            return Comparer.Default.Compare(x, y);
        }
    }
}