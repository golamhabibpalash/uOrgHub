using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_stock_transactions")]
public class StockTransaction : BaseEntity
{
    [Required] [MaxLength(30)] public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public StockTransactionType TransactionType { get; set; }
    public StockTransactionStatus Status { get; set; } = StockTransactionStatus.Draft;

    [Required] public Guid ItemVariantId { get; set; }
    public ItemVariant ItemVariant { get; set; } = null!;

    [Required] public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid? FromWarehouseId { get; set; }
    public Warehouse? FromWarehouse { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    [MaxLength(50)] public string? ReferenceNumber { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
