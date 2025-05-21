using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Domain.Entities.Others
{
    public class AddressType : CommonProps
    {
        public int Id { get; set; }
        public required string Name { get; set; } = default!;
        public virtual ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new HashSet<EmployeeAddress>();
    }
}
