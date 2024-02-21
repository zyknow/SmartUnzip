using CommunityToolkit.Mvvm.ComponentModel;
using SmartUnzip.Core;

namespace SmartUnzip.Avalonia;

[INotifyPropertyChanged]
public partial class UnzipPassword(string value, uint manualSort = 0, uint useCount = 0) : IUnzipPassword
{
    [ObservableProperty]
    private string _value = value;

    [ObservableProperty]
    private uint _useCount = useCount;

    [ObservableProperty]
    private uint _manualSort = manualSort;

    public void IncreaseUseCount()
    {
        UseCount++;
    }


    public override bool Equals(object? obj)
    {
        return obj is UnzipPassword password && Value == password.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}