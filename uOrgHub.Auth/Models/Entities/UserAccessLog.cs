using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_access_logs")]
public class UserAccessLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    [MaxLength(100)]
    public string? Username { get; set; }

    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Module { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(100)]
    public string? EntityId { get; set; }

    [MaxLength(10)]
    public string? HttpMethod { get; set; }

    [MaxLength(500)]
    public string? Endpoint { get; set; }

    public string? RequestBody { get; set; }

    public int ResponseStatusCode { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public long DurationMs { get; set; }

    public bool IsSuccess { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
