namespace SmartUnzip.Core;

public partial class PasswordModel : ObservableObject
{
    [ObservableProperty] string? _value;

    [ObservableProperty] int _useCount;
}