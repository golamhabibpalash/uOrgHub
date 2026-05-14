using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CreatePOItemDto(
    Guid ItemVariantId,
    decimal OrderedQuantity,
    decimal UnitPrice,
    decimal TaxPercent,
    decimal DiscountPercent,
    string? Notes
);

public record CreatePurchaseOrderDto(
    DateTime PODate,
    DateTime ExpectedDeliveryDate,
    Guid VendorId,
    Guid? QuotationId,
    Guid? PRId,
    string? PaymentTerms,
    string? DeliveryAddress,
    string? Notes,
    List<CreatePOItemDto> Items
);

public record UpdatePOItemDto(
    Guid? Id,
    Guid ItemVariantId,
    decimal OrderedQuantity,
    decimal UnitPrice,
    decimal TaxPercent,
    decimal DiscountPercent,
    string? Notes
);

public record UpdatePurchaseOrderDto(
    DateTime PODate,
    DateTime ExpectedDeliveryDate,
    Guid VendorId,
    Guid? QuotationId,
    Guid? PRId,
    string? PaymentTerms,
    string? DeliveryAddress,
    string? Notes,
    List<UpdatePOItemDto> Items
);

public class POItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class POResponseDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateTime PODate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public Guid? QuotationId { get; set; }
    public string? QuotationNumber { get; set; }
    public Guid? PRId { get; set; }
    public string? PRNumber { get; set; }
    public POStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<POItemResponseDto> Items { get; set; } = new();
}
