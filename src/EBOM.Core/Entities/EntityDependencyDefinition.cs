namespace EBOM.Core.Entities;

public class EntityDependencyDefinition : BaseEntity
{
    public int EntityDependencyID { get; set; }
    public int TemplateRevisionID { get; set; }
    public int EntityID { get; set; }
    public int DependentEntityID { get; set; }
    public int EntityOrder { get; set; }
    public bool IsValueType { get; set; }
    
    // Navigation properties
    public virtual EntityTemplateRevision? TemplateRevision { get; set; }
    public virtual Entity? Entity { get; set; }
    public virtual Entity? DependentEntity { get; set; }
}