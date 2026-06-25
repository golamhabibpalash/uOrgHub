using System.ComponentModel.DataAnnotations;

namespace OrgHub.Application.Features.HRM.Departments.DTOs;

public class CreateDepartmentDtos
{
    public required string Name { get; set; }
    [StringLength(50)]
    public required string Code { get; set; }
    public int? ParentDepartmentId { get; set; }
    public int? HeadEmployeeId { get; set; }
}
