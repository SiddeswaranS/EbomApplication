namespace EBOM.Core.Entities;

public class UserMaster
{
    public int UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; } = 1;
}