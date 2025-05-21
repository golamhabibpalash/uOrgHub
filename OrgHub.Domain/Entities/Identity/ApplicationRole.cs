using Microsoft.AspNetCore.Identity;

namespace OrgHub.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole<Guid>, ICommonProps
    {
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int LastUpdatedBy { get; set; }
        public ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();

    }
}
