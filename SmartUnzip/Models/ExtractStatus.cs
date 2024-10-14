namespace SmartUnzip.Models;

public enum ExtractStatus
{
    None,
    Testing,
    Tested,
    Extracting,
    ExtractSucceeded,
    Finished,
    Error
}