namespace EBOM.Core.Enums;

public enum DataType
{
    String = 1,
    Integer = 2,
    Date = 3,
    Boolean = 4,
    Range = 5,      // Single colon format (e.g., 10:100)
    RangeSet = 6    // Double colon format (e.g., 10:100:10)
}