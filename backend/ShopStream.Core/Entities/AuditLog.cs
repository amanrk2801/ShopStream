namespace ShopStream.Core.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string Event { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
