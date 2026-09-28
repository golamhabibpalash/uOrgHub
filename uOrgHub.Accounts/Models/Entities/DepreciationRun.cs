using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

/// <summary>
/// One month's depreciation for every asset that had some due, posted as a single journal entry
/// (Dr Depreciation Expense / Cr Accumulated Depreciation, per asset).
/// </summary>
[Table("acc_depreciation_runs")]
public class DepreciationRun : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    [Required][MaxLength(30)] public string RunNumber { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public DateTime PeriodEndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
    public DepreciationRunStatus Status { get; set; } = DepreciationRunStatus.Posted;
    [MaxLength(500)] public string? Notes { get; set; }

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    /// <summary>
    /// A run is never edited after posting, so the audit UpdatedBy of a reversed run is the person
    /// who reversed it — no separate column needed.
    /// </summary>
    public DateTime? ReversedAt { get; set; }

    public ICollection<DepreciationRunLine> Lines { get; set; } = new List<DepreciationRunLine>();
}

[Table("acc_depreciation_run_lines")]
public class DepreciationRunLine : BaseEntity
{
    public Guid DepreciationRunId { get; set; }
    public DepreciationRun DepreciationRun { get; set; } = null!;

    public Guid FixedAssetId { get; set; }
    public FixedAsset FixedAsset { get; set; } = null!;

    /// <summary>Months charged — more than one when the asset was catching up on missed runs.</summary>
    public int Months { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal AccumulatedBefore { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal AccumulatedAfter { get; set; }

    /// <summary>The asset's LastDepreciationDate before this run, restored if the run is reversed.</summary>
    public DateTime? PreviousLastDepreciationDate { get; set; }
}
