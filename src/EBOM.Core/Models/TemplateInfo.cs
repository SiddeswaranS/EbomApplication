namespace EBOM.Core.Models;

public class TemplateInfo
{
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int ActiveRevision { get; set; }
    public DateTime LastUpdated { get; set; }
    public int ColumnCount { get; set; }
}