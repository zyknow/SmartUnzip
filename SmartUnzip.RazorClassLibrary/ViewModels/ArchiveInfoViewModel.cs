using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartUnzip.Core.Models;

namespace SmartUnzip.ViewModels;
[INotifyPropertyChanged]
public partial class ArchiveInfoViewModel
{
    [ObservableProperty]
    private ArchiveFileInfo _info;
}
