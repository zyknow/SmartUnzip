using Avalonia.Media;

namespace SmartUnzip.ViewModels;

public partial class UnzipTreeItemViewModel(string filePath) : ViewModelBase
{
    [ObservableProperty] private string _fileName = System.IO.Path.GetFileName(filePath);
    [ObservableProperty] private string _filePath = filePath;

    [NotifyPropertyChangedFor(nameof(ExtractColor))] [ObservableProperty]
    private ExtractStatus _extractStatus;

    [ObservableProperty] private string message;
    [ObservableProperty] bool isVolume;

    [ObservableProperty] private string password;

    [ObservableProperty] private string outputPath;

    public UnzipTreeItemViewModel? Parent;

    public IBrush ExtractColor => ExtractStatus switch
    {
        ExtractStatus.None => Brushes.Gray,
        ExtractStatus.Extracting => Brushes.Blue,
        ExtractStatus.Error => Brushes.Red,
        ExtractStatus.ExtractSucceeded => Brushes.LawnGreen,
        ExtractStatus.Testing => Brushes.Gold,
        ExtractStatus.Tested => Brushes.Cyan,
        _ => Brushes.Transparent
    };

    public ObservableCollection<UnzipTreeItemViewModel> Children { get; set; } = [];
}