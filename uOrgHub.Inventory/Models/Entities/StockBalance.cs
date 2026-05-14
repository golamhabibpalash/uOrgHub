using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_stock_balances")]
public class StockBalance : BaseEntity
{
    [Required] public Guid ItemVariantId { get; set; }
    public ItemVariant ItemVariant { get; set; } = null!;
    [Required] public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public decimal QuantityOnHand { get; set; } = 0;
    public decimal QuantityReserved { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
