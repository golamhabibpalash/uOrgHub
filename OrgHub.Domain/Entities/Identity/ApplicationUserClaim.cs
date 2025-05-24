using Microsoft.AspNetCore.Identity;

namespace OrgHub.Domain.Entities.Identity;

public class ApplicationUserClaim : IdentityUserClaim<Guid>
{
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public int LastUpdatedBy { get; set; }
}
