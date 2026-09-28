using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_warehouses")]
public class Warehouse : BaseEntity, ICompanyScoped
{
    // Sister-concern isolation (SISTER_CONCERN_PLAN.md). Unlike the Accounts/Procurement/Projects
    // anchors, StockBalance and StockTransaction here ALSO carry their own CompanyId rather than
    // inheriting scoping through WarehouseId alone — both have their own independent list/query
    // handlers (StockBalanceQueries.cs, StockTransactionQueries.cs) that query
    // `_context.Set<T>()` directly via BaseRepository, never joined through Warehouse, so
    // inheriting scoping "through the parent" the way BillLine/PurchaseOrderItem do would have
    // left them completely unfiltered.
    public Guid CompanyId { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(20)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string? Location { get; set; }
    [MaxLength(100)] public string? ContactPerson { get; set; }
    [MaxLength(20)] public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
}
