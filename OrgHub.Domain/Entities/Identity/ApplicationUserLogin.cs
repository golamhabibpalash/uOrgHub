using Microsoft.AspNetCore.Identity;

namespace OrgHub.Domain.Entities.Identity;

public class ApplicationUserLogin: IdentityUserLogin<Guid>, ICommonProps
{
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public int LastUpdatedBy { get; set; }
    public ApplicationUser? ApplicationUser { get; set; } // Optional: navigation
}