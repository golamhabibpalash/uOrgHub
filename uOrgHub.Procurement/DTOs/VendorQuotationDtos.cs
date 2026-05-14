using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CreateQuotationItemDto(
    Guid RFQItemId,
    Guid ItemVariantId,
    decimal QuotedQuantity,
    decimal UnitPrice,
    string? Notes
);

public record CreateVendorQuotationDto(
    Guid RFQId,
    Guid VendorId,
    DateTime QuotationDate,
    DateTime ValidUntil,
    int DeliveryDays,
    string? PaymentTerms,
    string? Notes,
    List<CreateQuotationItemDto> Items
);

public record UpdateQuotationItemDto(
    Guid? Id,
    Guid RFQItemId,
    Guid ItemVariantId,
    decimal QuotedQuantity,
    decimal UnitPrice,
    string? Notes
);

public record UpdateVendorQuotationDto(
    DateTime QuotationDate,
    DateTime ValidUntil,
    QuotationStatus Status,
    int DeliveryDays,
    string? PaymentTerms,
    string? Notes,
    List<UpdateQuotationItemDto> Items
);

public class QuotationItemResponseDto
{
    public Guid Id { get; set; }
    public Guid RFQItemId { get; set; }
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public decimal QuotedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class VendorQuotationResponseDto
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public Guid RFQId { get; set; }
    public string RFQNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public QuotationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal TotalAmount { get; set; }
    public int DeliveryDays { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuotationItemResponseDto> Items { get; set; } = new();
}
