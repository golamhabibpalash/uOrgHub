using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_user_roles")]
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public ApplicationRole Role { get; set; } = default!;
}
