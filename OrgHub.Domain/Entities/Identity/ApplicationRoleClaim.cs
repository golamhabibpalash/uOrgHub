using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Domain.Entities.Identity
{
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>, ICommonProps
    {
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int LastUpdatedBy { get; set; }
        public ApplicationRole? ApplicationRole { get; set; } // Optional: navigation
    }
}
