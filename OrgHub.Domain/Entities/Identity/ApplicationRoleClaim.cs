using Microsoft.AspNetCore.Identity;

namespace OrgHub.Domain.Entities.Identity;

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>, ICommonProps
{
    public DateTime CreatedDate { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public Guid? LastUpdatedBy { get; set; }
    public ApplicationRole? ApplicationRole { get; set; } // Optional: navigation
}
