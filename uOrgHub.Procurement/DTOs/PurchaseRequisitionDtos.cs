using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CreatePRItemDto(
    Guid ItemVariantId,
    Guid WarehouseId,
    decimal RequestedQuantity,
    decimal EstimatedUnitCost,
    string? Notes
);

public record CreatePRDto(
    DateTime PRDate,
    DateTime RequiredDate,
    Guid DepartmentId,
    Guid RequestedById,
    string? Purpose,
    string? Notes,
    List<CreatePRItemDto> Items
);

public record UpdatePRItemDto(
    Guid? Id,
    Guid ItemVariantId,
    Guid WarehouseId,
    decimal RequestedQuantity,
    decimal EstimatedUnitCost,
    string? Notes
);

public record UpdatePRDto(
    DateTime PRDate,
    DateTime RequiredDate,
    Guid DepartmentId,
    Guid RequestedById,
    string? Purpose,
    string? Notes,
    List<UpdatePRItemDto> Items
);

public record RejectPRDto(string Reason);

public class PRItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public decimal EstimatedTotalCost { get; set; }
    public string? Notes { get; set; }
}

public class PRResponseDto
{
    public Guid Id { get; set; }
    public string PRNumber { get; set; } = string.Empty;
    public DateTime PRDate { get; set; }
    public DateTime RequiredDate { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid RequestedById { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public PRStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Guid? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PRItemResponseDto> Items { get; set; } = new();
}
