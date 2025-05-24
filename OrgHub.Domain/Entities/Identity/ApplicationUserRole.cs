using Microsoft.AspNetCore.Identity;

namespace OrgHub.Domain.Entities.Identity
{
    public class ApplicationUserRole : IdentityUserRole<Guid>, ICommonProps
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid CreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public Guid? LastUpdatedBy { get; set; }
    }
}
