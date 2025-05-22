using System.ComponentModel.DataAnnotations;

namespace OrgHub.Domain.Entities.HRM;

public class HRM_Department : CommonProps
{
    public int Id { get; set; }
    public required string Name { get; set; }
    [StringLength(50)]
    public required string Code { get; set; }
    public int? ParentDepartmentId { get; set; }
    public int? HeadEmployeeId { get; set; }
    public bool IsActive { get; set; }
    public ICollection<HRM_Employee>? Employees { get; set; }
}
