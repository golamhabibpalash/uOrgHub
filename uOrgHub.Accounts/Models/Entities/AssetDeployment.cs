using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

/// <summary>
/// One stay of a fixed asset on a project site, from deployment to return, and how the project
/// pays for it. An asset has at most one open deployment at a time.
///
/// The project is held as a bare <see cref="ProjectId"/> (Accounts cannot reference the Projects
/// module); the money side goes through the project's own <see cref="CostCenter"/>, which is what
/// project costing reads.
/// </summary>
[Table("acc_asset_deployments")]
public class AssetDeployment : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    public Guid FixedAssetId { get; set; }
    public FixedAsset FixedAsset { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Guid CostCenterId { get; set; }
    public CostCenter CostCenter { get; set; } = null!;

    /// <summary>First day on site (charged).</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Last day on site (charged); null while the asset is still there.</summary>
    public DateTime? EndDate { get; set; }

    public DeploymentChargeMode ChargeMode { get; set; }
    public HireRateUnit? RateUnit { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? Rate { get; set; }

    /// <summary>Where the asset was before going to site, restored as its location on return.</summary>
    [MaxLength(200)] public string? PreviousLocation { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    [MaxLength(500)] public string? ReturnNotes { get; set; }

    [NotMapped] public bool IsOpen => EndDate is null;
}
