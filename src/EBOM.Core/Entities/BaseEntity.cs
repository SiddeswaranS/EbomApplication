namespace EBOM.Core.Entities;

public abstract class BaseEntity
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; } = 1;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}