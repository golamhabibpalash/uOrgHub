using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs;

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DepartmentType Type { get; set; } = DepartmentType.Other;
    public bool IsActive { get; set; } = true;
    public Guid? ParentDepartmentId { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
}
