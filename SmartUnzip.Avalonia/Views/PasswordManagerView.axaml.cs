using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SmartUnzip.Avalonia.ViewModels;
using SmartUnzip.Core.Models;

namespace SmartUnzip.Avalonia.Views;

public partial class PasswordManagerView : UserControl
{
    public PasswordManagerViewModel vm { get; set; }

    
    public PasswordManagerView()
    {
        InitializeComponent();
        vm = App.ServiceProvider.GetRequiredService<PasswordManagerViewModel>();
        
        
        var dataGridSortDescription = DataGridSortDescription.FromPath(nameof(UnzipPassword.Value), ListSortDirection.Ascending, new ReversedStringComparer());
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
    
    private void NumericUpDown_OnTemplateApplied(object sender, TemplateAppliedEventArgs e)
    {
        // We want to focus the TextBox of the NumericUpDown. To do so we search for this control when the template
        // is applied, but we postpone the action until the control is actually loaded. 
        if (e.NameScope.Find<TextBox>("PART_TextBox") is {} textBox)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                textBox.Focus();
                textBox.SelectAll();
            }, DispatcherPriority.Loaded);
        }
    }
}