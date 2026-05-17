using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_user_companies")]
public class UserCompany : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }

    public bool IsDefault { get; set; } = false;

    [MaxLength(100)]
    public string RoleInCompany { get; set; } = "Member";

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = default!;
    public Company Company { get; set; } = default!;
}
