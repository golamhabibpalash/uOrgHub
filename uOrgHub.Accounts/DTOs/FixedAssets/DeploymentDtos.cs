using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.FixedAssets;

// ── Deployments ────────────────────────────────────────────────────────────

public class DeployAssetDto
{
    public Guid FixedAssetId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime StartDate { get; set; }
    public DeploymentChargeMode ChargeMode { get; set; }
    /// <summary>Required when <see cref="ChargeMode"/> is HireRate; ignored otherwise.</summary>
    public HireRateUnit? RateUnit { get; set; }
    public decimal? Rate { get; set; }
    public string? Notes { get; set; }
}

public class ReturnAssetDto
{
    /// <summary>Last day on site (inclusive).</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Where the asset goes back to; defaults to where it was before deployment.</summary>
    public string? ReturnLocation { get; set; }
    public FixedAssetStatus ReturnStatus { get; set; } = FixedAssetStatus.Active;
    public string? ReturnNotes { get; set; }
}

public class AssetDeploymentResponseDto
{
    public Guid Id { get; set; }
    public Guid FixedAssetId { get; set; }
    public string FixedAssetAssetCode { get; set; } = string.Empty;
    public string FixedAssetName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid CostCenterId { get; set; }
    /// <summary>The project's cost center carries the project's name.</summary>
    public string? CostCenterName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsOpen => EndDate is null;
    public DeploymentChargeMode ChargeMode { get; set; }
    public HireRateUnit? RateUnit { get; set; }
    public decimal? Rate { get; set; }
    public string? Notes { get; set; }
    public string? ReturnNotes { get; set; }
    /// <summary>Last day covered by a posted hire charge run; null when nothing has been charged yet.</summary>
    public DateTime? ChargedUpTo { get; set; }
    public decimal TotalHireCharged { get; set; }
}

// ── Hire charge runs ───────────────────────────────────────────────────────

public class PostHireChargeRunDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Notes { get; set; }
}

public class HireChargeLineDto
{
    public Guid AssetDeploymentId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Days { get; set; }
    public HireRateUnit RateUnit { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}

public class HireChargePreviewDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<HireChargeLineDto> Lines { get; set; } = new();
    /// <summary>Set when the charges would take a project past its cost ceiling. Reported, never blocking.</summary>
    public string? Warning { get; set; }
}

public class HireChargeRunResponseDto
{
    public Guid Id { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalAmount { get; set; }
    public HireChargeRunStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? JournalEntryId { get; set; }
    public string? JournalEntryEntryNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ReversedAt { get; set; }
    public string? ReversedBy { get; set; }
    public int DeploymentCount => Lines.Count;
    public List<HireChargeLineDto> Lines { get; set; } = new();
    public string? Warning { get; set; }
}
