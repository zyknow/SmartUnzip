namespace SmartUnzip.Core.Models;

public class UnzipPassword(string value, int manualSort = int.MaxValue, int useCount = 0)
{
    public string Value { get; } = value;
    public int UseCount { get; protected set; } = useCount;
    public int ManualSort { get; set; } = manualSort;


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