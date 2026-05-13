namespace uOrgHub.HR.DTOs;

public class CreateDesignationDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public Guid DepartmentId { get; set; }
    public Guid? ParentDesignationId { get; set; }
    public Guid? SalaryGradeId { get; set; }
}
