using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CreateGRNItemDto(
    Guid POItemId,
    Guid ItemVariantId,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RejectedQuantity,
    decimal UnitCost,
    string? Notes
);

public record CreateGRNDto(
    DateTime GRNDate,
    Guid POId,
    Guid WarehouseId,
    Guid ReceivedById,
    string? Notes,
    string? InvoiceNumber,
    DateTime? InvoiceDate,
    List<CreateGRNItemDto> Items
);

public record UpdateGRNItemDto(
    Guid? Id,
    Guid POItemId,
    Guid ItemVariantId,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RejectedQuantity,
    decimal UnitCost,
    string? Notes
);

public record UpdateGRNDto(
    DateTime GRNDate,
    Guid WarehouseId,
    Guid ReceivedById,
    string? Notes,
    string? InvoiceNumber,
    DateTime? InvoiceDate,
    List<UpdateGRNItemDto> Items
);

public class GRNItemResponseDto
{
    public Guid Id { get; set; }
    public Guid POItemId { get; set; }
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}

public class GRNResponseDto
{
    public Guid Id { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime GRNDate { get; set; }
    public Guid POId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public Guid ReceivedById { get; set; }
    public string ReceivedByName { get; set; } = string.Empty;
    public GRNStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? Notes { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GRNItemResponseDto> Items { get; set; } = new();
}
