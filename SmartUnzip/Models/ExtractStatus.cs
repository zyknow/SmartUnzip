using System.ComponentModel;

namespace SmartUnzip.Models;

public enum ExtractStatus
{
    None,
    Testing,
    Tested,
    Extracting,
    ExtractSucceeded,
    Error
}