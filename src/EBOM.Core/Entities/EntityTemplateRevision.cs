namespace EBOM.Core.Entities;

public class EntityTemplateRevision : BaseEntity
{
    public int TemplateRevisionID { get; set; }
    public int EntityID { get; set; }
    public int TemplateRevisionNumber { get; set; }
    public string? TemplateRevisionDescription { get; set; }
    
    // Navigation properties
    public virtual Entity? Entity { get; set; }
    public virtual ICollection<EntityDependencyDefinition>? DependencyDefinitions { get; set; }
    public virtual ICollection<EntityDataRevision>? DataRevisions { get; set; }
}