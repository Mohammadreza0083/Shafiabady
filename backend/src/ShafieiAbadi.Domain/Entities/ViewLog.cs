using ShafieiAbadi.Domain.Common;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Domain.Entities;

public class ViewLog : BaseEntity, IAggregateRoot
{
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public bool IsWithin24Hours(DateTime checkTime)
    {
        return (checkTime - ViewedAt).TotalHours < 24;
    }
}
