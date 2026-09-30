using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

/// <summary>
/// A class of fixed asset (Heavy Machinery, Vehicles, IT Equipment...). Carries the depreciation
/// defaults a new asset starts from and the three GL accounts every asset in it posts to.
/// </summary>
[Table("acc_asset_categories")]
public class AssetCategory : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }

    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int UsefulLifeMonths { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal SalvageValuePercent { get; set; }

    /// <summary>Balance-sheet account holding the assets at cost, e.g. "Plant &amp; Machinery".</summary>
    public Guid AssetAccountId { get; set; }
    public ChartOfAccount AssetAccount { get; set; } = null!;

    /// <summary>Contra-asset credited by each depreciation run.</summary>
    public Guid AccumulatedDepreciationAccountId { get; set; }
    public ChartOfAccount AccumulatedDepreciationAccount { get; set; } = null!;

    /// <summary>Expense debited by each depreciation run.</summary>
    public Guid DepreciationExpenseAccountId { get; set; }
    public ChartOfAccount DepreciationExpenseAccount { get; set; } = null!;

    /// <summary>
    /// Expense debited on the project's cost center for internal equipment hire. Optional until an
    /// asset in this category is deployed on a hire rate.
    /// </summary>
    public Guid? HireExpenseAccountId { get; set; }
    public ChartOfAccount? HireExpenseAccount { get; set; }

    /// <summary>
    /// Income credited with the same hire charge. Company-wide it cancels the hire expense, so
    /// internal hire moves cost onto projects without inflating the company's own profit.
    /// </summary>
    public Guid? HireRecoveryAccountId { get; set; }
    public ChartOfAccount? HireRecoveryAccount { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<FixedAsset> Assets { get; set; } = new List<FixedAsset>();
}
