using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_role_claims")]
public class RoleClaim : BaseEntity
{
    public Guid RoleId { get; set; }

    public Guid ClaimId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    public ApplicationRole Role { get; set; } = default!;
    public ApplicationClaim Claim { get; set; } = default!;
}
