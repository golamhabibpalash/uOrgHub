using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CreateRFQItemDto(
    Guid ItemVariantId,
    decimal RequestedQuantity,
    string? Notes
);

public record CreateRFQDto(
    DateTime RFQDate,
    DateTime ClosingDate,
    Guid? PRId,
    string Title,
    string? Description,
    string? Notes,
    List<CreateRFQItemDto> Items
);

public record UpdateRFQItemDto(
    Guid? Id,
    Guid ItemVariantId,
    decimal RequestedQuantity,
    string? Notes
);

public record UpdateRFQDto(
    DateTime RFQDate,
    DateTime ClosingDate,
    Guid? PRId,
    string Title,
    string? Description,
    RFQStatus Status,
    string? Notes,
    List<UpdateRFQItemDto> Items
);

public class RFQItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class RFQResponseDto
{
    public Guid Id { get; set; }
    public string RFQNumber { get; set; } = string.Empty;
    public DateTime RFQDate { get; set; }
    public DateTime ClosingDate { get; set; }
    public Guid? PRId { get; set; }
    public string? PRNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RFQStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RFQItemResponseDto> Items { get; set; } = new();
}
