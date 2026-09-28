using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

/// <summary>
/// One individually tracked capital item — an excavator, a vibrator machine, a vehicle. Unlike an
/// inventory item it is never issued or consumed: it keeps its identity, sits on the balance sheet
/// at cost and loses value through depreciation.
///
/// The register does not post the acquisition itself. The purchase reaches the ledger through the
/// vendor bill (a line on the category's asset account) or, for assets owned before go-live,
/// through the account's opening balance; <see cref="BillId"/> links the two.
/// </summary>
[Table("acc_fixed_assets")]
public class FixedAsset : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    [Required][MaxLength(30)]  public string AssetCode { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }

    public Guid CategoryId { get; set; }
    public AssetCategory Category { get; set; } = null!;

    [MaxLength(100)] public string? Manufacturer { get; set; }
    [MaxLength(100)] public string? Model { get; set; }
    [MaxLength(100)] public string? SerialNumber { get; set; }
    [MaxLength(100)] public string? ChassisNumber { get; set; }
    [MaxLength(100)] public string? EngineNumber { get; set; }
    [MaxLength(50)]  public string? RegistrationNumber { get; set; }

    public DateTime PurchaseDate { get; set; }
    /// <summary>First month depreciation is charged for (normally the month it was put to use).</summary>
    public DateTime DepreciationStartDate { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal PurchaseCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;

    /// <summary>Depreciation already charged before this system took over (existing assets).</summary>
    [Column(TypeName = "decimal(18,2)")] public decimal OpeningAccumulatedDepreciation { get; set; }
    /// <summary>Opening amount plus every posted run; reduced again when a run is reversed.</summary>
    [Column(TypeName = "decimal(18,2)")] public decimal AccumulatedDepreciation { get; set; }
    /// <summary>Month-end the asset has been depreciated up to; null until the first charge.</summary>
    public DateTime? LastDepreciationDate { get; set; }

    [NotMapped] public decimal BookValue => PurchaseCost - AccumulatedDepreciation;

    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public Guid? BillId { get; set; }
    public Bill? Bill { get; set; }

    /// <summary>Stamped on the depreciation expense lines, so the charge lands on that cost center.</summary>
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    [MaxLength(200)] public string? Location { get; set; }
    public FixedAssetStatus Status { get; set; } = FixedAssetStatus.Active;
    [MaxLength(1000)] public string? Notes { get; set; }
}
