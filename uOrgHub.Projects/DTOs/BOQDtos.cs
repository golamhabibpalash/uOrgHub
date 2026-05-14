using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateBOQDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateBOQItemDto> Items { get; set; } = new();
}

public class UpdateBOQDto
{
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ApproveBOQDto
{
    public Guid ApprovedById { get; set; }
}

public class CreateBOQItemDto
{
    public Guid? ItemVariantId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal UnitRate { get; set; }
    public int Sequence { get; set; }
}

public class UpdateBOQItemDto
{
    public Guid? ItemVariantId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal ActualAmount { get; set; }
    public int Sequence { get; set; }
}

public class BOQResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string BOQNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BOQStatus Status { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public List<BOQItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class BOQItemResponseDto
{
    public Guid Id { get; set; }
    public Guid BOQId { get; set; }
    public Guid? ItemVariantId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal EstimatedAmount { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal ActualAmount { get; set; }
    public int Sequence { get; set; }
}
