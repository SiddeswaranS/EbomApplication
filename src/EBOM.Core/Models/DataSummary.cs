namespace EBOM.Core.Models;

public class DataSummary
{
    public string EntityName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ActiveRevision { get; set; }
    public DateTime LastUpload { get; set; }
    public List<string> UniqueValues { get; set; } = new();
}

public class DataRevisionInfo
{
    public int DataRevisionId { get; set; }
    public int DataRevisionNumber { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int RowCount { get; set; }
}