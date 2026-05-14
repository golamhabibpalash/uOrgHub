using uOrgHub.Inventory.Models.Enums;

namespace uOrgHub.Inventory.DTOs;

public record CreateStockTransactionDto(
    DateTime TransactionDate,
    StockTransactionType TransactionType,
    Guid ItemVariantId,
    Guid WarehouseId,
    Guid? FromWarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber,
    string? Notes);

public record UpdateStockTransactionDto(
    DateTime TransactionDate,
    Guid ItemVariantId,
    Guid WarehouseId,
    Guid? FromWarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber,
    string? Notes);

public class StockTransactionResponseDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType.ToString();
    public StockTransactionStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public Guid? FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Quantity * UnitCost;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
