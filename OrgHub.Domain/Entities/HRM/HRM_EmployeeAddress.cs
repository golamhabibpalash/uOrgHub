using OrgHub.Domain.Entities.Others;

namespace OrgHub.Domain.Entities.HRM;

public class HRM_EmployeeAddress : CommonProps
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public required string AreaAddress { get; set; }
    public int AddressTypeId { get; set; }
    public int UpazilaId { get; set; }
    public required virtual Upazila Upazila { get; set; }
    public virtual required HRM_Employee Employee { get; set; }
    public virtual required AddressType AddressType { get; set; }
}
