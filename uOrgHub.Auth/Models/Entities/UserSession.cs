using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_user_sessions")]
public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [MaxLength(500)]
    public string? SessionToken { get; set; }

    [MaxLength(500)]
    public string? DeviceInfo { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(200)]
    public string? Browser { get; set; }

    [MaxLength(100)]
    public string? Os { get; set; }

    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public DateTime? LogoutAt { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(20)]
    public string? LogoutReason { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
