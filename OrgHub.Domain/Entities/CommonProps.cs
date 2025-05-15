using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Domain.Entities
{
    public class CommonProps
    {
        public required DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int LastUpdatedBy { get; set; } = 0;
    }
}
