namespace EBOM.Core.Entities;

public class EntityDataRevision : BaseEntity
{
    public int DataRevisionId { get; set; }
    public int TemplateRevisionId { get; set; }
    public int EntityId { get; set; }
    public int DataRevisionNumber { get; set; }
    public string? DataRevisionDescription { get; set; }
    
    // Navigation properties
    public virtual EntityTemplateRevision? TemplateRevision { get; set; }
    public virtual Entity? Entity { get; set; }
}