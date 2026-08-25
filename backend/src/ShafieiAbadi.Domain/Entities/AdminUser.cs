using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

public class AdminUser : BaseEntity, IAggregateRoot
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = "دکتر محمد حسین شفیع‌آبادی";
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}
