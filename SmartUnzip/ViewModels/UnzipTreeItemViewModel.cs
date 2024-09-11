using Avalonia.Media;

namespace SmartUnzip.ViewModels;

public partial class UnzipTreeItemViewModel : ViewModelBase
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _filePath;

    [NotifyPropertyChangedFor(nameof(ExtractColor))] [ObservableProperty]
    private ExtractStatus _extractStatus;

    [ObservableProperty] private string message;
    [ObservableProperty] bool isVolume;

    [ObservableProperty] private string passowrd;

    [ObservableProperty] private string outputPath;

    public UnzipTreeItemViewModel(string filePath)
    {
        FileName = System.IO.Path.GetFileName(filePath);
        FilePath = filePath;
    }

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