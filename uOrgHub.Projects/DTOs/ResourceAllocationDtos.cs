using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateResourceAllocationDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? UnitOfMeasure { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}

public class UpdateResourceAllocationDto
{
    public Guid? WBSId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? UnitOfMeasure { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public ResourceAllocationStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class ResourceAllocationResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public string? EquipmentCode { get; set; }
    public string? UnitOfMeasure { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal PlannedCost { get; set; }
    public decimal ActualCost { get; set; }
    public ResourceAllocationStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
