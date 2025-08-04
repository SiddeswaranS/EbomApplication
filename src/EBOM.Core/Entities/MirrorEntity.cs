namespace EBOM.Core.Entities;

public class MirrorEntity
{
    public int EntityId { get; set; }
    public int MirrorEntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; } = 1;
    
    // Navigation properties
    public virtual Entity? Entity { get; set; }
    public virtual Entity? MirrorEntityNavigation { get; set; }
}