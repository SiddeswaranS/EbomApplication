using EBOM.Core.Enums;

namespace EBOM.Core.Entities;

public class Entity : BaseEntity
{
    public int EntityID { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public string? EntityDescription { get; set; }
    public string EntityType { get; set; } = string.Empty; // ISR, PSR, CSR, CMN
    public int DataTypeID { get; set; }
    
    // Navigation properties
    public virtual EntityDataType? DataType { get; set; }
    public virtual ICollection<EntityValue>? EntityValues { get; set; }
    public virtual ICollection<EntityTemplateRevision>? TemplateRevisions { get; set; }
    public virtual ICollection<MirrorEntity>? MirrorEntities { get; set; }
    public virtual ICollection<MirrorEntity>? MirroredByEntities { get; set; }
}