using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateMaterialRequestDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public Guid RequestedById { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime RequiredDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateMaterialRequestItemDto> Items { get; set; } = new();
}

public class UpdateMaterialRequestDto
{
    public Guid? WBSId { get; set; }
    public DateTime RequiredDate { get; set; }
    public string? Notes { get; set; }
}

public class ApproveMaterialRequestDto
{
    public Guid ApprovedById { get; set; }
    public List<ApproveItemQuantityDto> ApprovedItems { get; set; } = new();
}

public class ApproveItemQuantityDto
{
    public Guid ItemId { get; set; }
    public decimal ApprovedQuantity { get; set; }
}

public class CreateMaterialRequestItemDto
{
    public Guid ItemVariantId { get; set; }
    public Guid? BOQItemId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? Notes { get; set; }
}

public class MaterialRequestResponseDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public Guid RequestedById { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime RequiredDate { get; set; }
    public MaterialRequestStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public List<MaterialRequestItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class MaterialRequestItemResponseDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ItemVariantId { get; set; }
    public Guid? BOQItemId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal ApprovedQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? Notes { get; set; }
}
