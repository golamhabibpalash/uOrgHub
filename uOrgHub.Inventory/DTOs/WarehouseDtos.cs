namespace uOrgHub.Inventory.DTOs;

// Warehouse
public record CreateWarehouseDto(string Name, string Code, string? Location, string? ContactPerson, string? ContactPhone);
public record UpdateWarehouseDto(string Name, string Code, string? Location, string? ContactPerson, string? ContactPhone, bool IsActive);

public class WarehouseResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// StockBalance
public class StockBalanceResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemVariantId { get; set; }
    public string VariantSKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string ItemBaseName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    public DateTime LastUpdated { get; set; }
}
