using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

/// <summary>
/// Internal equipment hire for a chosen date range, posted as one journal entry: per deployment,
/// Dr Equipment Hire (on the project's cost center) / Cr Internal Equipment Recovery.
/// Posted runs never overlap, so no day on site is charged twice.
/// </summary>
[Table("acc_hire_charge_runs")]
public class HireChargeRun : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    [Required][MaxLength(30)] public string RunNumber { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
    public HireChargeRunStatus Status { get; set; } = HireChargeRunStatus.Posted;
    [MaxLength(500)] public string? Notes { get; set; }

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    /// <summary>A run is never edited after posting, so UpdatedBy of a reversed run is who reversed it.</summary>
    public DateTime? ReversedAt { get; set; }

    public ICollection<HireChargeRunLine> Lines { get; set; } = new List<HireChargeRunLine>();
}

[Table("acc_hire_charge_run_lines")]
public class HireChargeRunLine : BaseEntity
{
    public Guid HireChargeRunId { get; set; }
    public HireChargeRun HireChargeRun { get; set; } = null!;

    public Guid AssetDeploymentId { get; set; }
    public AssetDeployment AssetDeployment { get; set; } = null!;

    /// <summary>The part of the run's range this deployment was actually on site.</summary>
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Days { get; set; }

    public HireRateUnit RateUnit { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Rate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
}
