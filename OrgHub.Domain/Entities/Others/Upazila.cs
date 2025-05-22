using System.ComponentModel.DataAnnotations;

namespace OrgHub.Domain.Entities.Others
{
    public class Upazila : CommonProps
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }

        [StringLength(100)]
        public required string Name { get; set; }

        public virtual required District District { get; set; }
    }
}
