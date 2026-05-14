using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_user_claims")]
public class UserClaim : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ClaimId { get; set; }

    public bool IsGranted { get; set; } = true;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public ApplicationClaim Claim { get; set; } = default!;
}
