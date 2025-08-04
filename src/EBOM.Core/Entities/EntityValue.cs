namespace EBOM.Core.Entities;

public class EntityValue : BaseEntity
{
    public int EntityValueId { get; set; }
    public int EntityId { get; set; }
    public string EntityObjValue { get; set; } = string.Empty;
    public byte[]? EntityObjValueHash { get; set; }
    
    // Navigation property
    public virtual Entity? Entity { get; set; }
}