using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_two_factor_otps")]
public class TwoFactorOTP
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(10)]
    public string OTPCode { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string OTPType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Channel { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SentTo { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; }

    public int AttemptCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = default!;
}
