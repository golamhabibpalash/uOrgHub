namespace uOrgHub.Accounts.DTOs.CostCenter;

public class CreateCostCenterDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class UpdateCostCenterDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ProjectId { get; set; }
    public bool IsActive { get; set; }
}

public class CostCenterResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public string? ParentCostCenterName { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ProjectId { get; set; }
}
