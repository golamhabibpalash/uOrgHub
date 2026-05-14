using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_refresh_tokens")]
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    [MaxLength(200)]
    public string? RevokedReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? CreatedByIp { get; set; }

    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
