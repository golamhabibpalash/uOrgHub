using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.FixedAssets;

// ── Asset categories ───────────────────────────────────────────────────────

public class CreateAssetCategoryDto
{
    /// <summary>Optional; generated as AC-0001… when blank.</summary>
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int UsefulLifeMonths { get; set; }
    public decimal SalvageValuePercent { get; set; }
    public Guid AssetAccountId { get; set; }
    public Guid AccumulatedDepreciationAccountId { get; set; }
    public Guid DepreciationExpenseAccountId { get; set; }
    public Guid? HireExpenseAccountId { get; set; }
    public Guid? HireRecoveryAccountId { get; set; }
}

public class UpdateAssetCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal SalvageValuePercent { get; set; }
    public Guid AssetAccountId { get; set; }
    public Guid AccumulatedDepreciationAccountId { get; set; }
    public Guid DepreciationExpenseAccountId { get; set; }
    public Guid? HireExpenseAccountId { get; set; }
    public Guid? HireRecoveryAccountId { get; set; }
    public bool IsActive { get; set; }
}

public class AssetCategoryResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal SalvageValuePercent { get; set; }
    public Guid AssetAccountId { get; set; }
    public string? AssetAccountName { get; set; }
    public Guid AccumulatedDepreciationAccountId { get; set; }
    public string? AccumulatedDepreciationAccountName { get; set; }
    public Guid DepreciationExpenseAccountId { get; set; }
    public string? DepreciationExpenseAccountName { get; set; }
    public Guid? HireExpenseAccountId { get; set; }
    public string? HireExpenseAccountName { get; set; }
    public Guid? HireRecoveryAccountId { get; set; }
    public string? HireRecoveryAccountName { get; set; }
    public bool IsActive { get; set; }
}

// ── Fixed assets ───────────────────────────────────────────────────────────

public class CreateFixedAssetDto
{
    /// <summary>Optional; generated from the document numbering sequence when blank.</summary>
    public string? AssetCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    /// <summary>Defaults to the purchase date when omitted.</summary>
    public DateTime? DepreciationStartDate { get; set; }
    public decimal PurchaseCost { get; set; }
    /// <summary>Defaults to the category's salvage % of cost when omitted.</summary>
    public decimal? SalvageValue { get; set; }
    /// <summary>Defaults to the category's useful life when omitted.</summary>
    public int? UsefulLifeMonths { get; set; }
    /// <summary>Defaults to the category's method when omitted.</summary>
    public DepreciationMethod? DepreciationMethod { get; set; }
    /// <summary>For an asset owned before go-live: depreciation already charged elsewhere.</summary>
    public decimal OpeningAccumulatedDepreciation { get; set; }
    /// <summary>Month-end the opening amount covers. Required when an opening amount is given.</summary>
    public DateTime? OpeningDepreciatedUpTo { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? BillId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? Location { get; set; }
    public FixedAssetStatus Status { get; set; } = FixedAssetStatus.Active;
    public string? Notes { get; set; }
}

public class UpdateFixedAssetDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime DepreciationStartDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? BillId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? Location { get; set; }
    public FixedAssetStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class FixedAssetResponseDto
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime DepreciationStartDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public DateTime? LastDepreciationDate { get; set; }
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? BillId { get; set; }
    public string? BillBillNumber { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public string? Location { get; set; }
    public FixedAssetStatus Status { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// True once a depreciation run has charged this asset. Cost, life, method, salvage and start
    /// date are then locked — changing them would silently rewrite depreciation already booked.
    /// </summary>
    public bool HasPostedDepreciation { get; set; }
}

public class FixedAssetDepreciationHistoryDto
{
    public Guid DepreciationRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public DateTime PeriodEndDate { get; set; }
    public DepreciationRunStatus Status { get; set; }
    public int Months { get; set; }
    public decimal Amount { get; set; }
    public decimal AccumulatedAfter { get; set; }
}

// ── Depreciation runs ──────────────────────────────────────────────────────

public class PostDepreciationRunDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string? Notes { get; set; }
}

public class DepreciationLineDto
{
    public Guid FixedAssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Months { get; set; }
    public decimal Amount { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal AccumulatedBefore { get; set; }
    public decimal AccumulatedAfter { get; set; }
    public decimal BookValueAfter => PurchaseCost - AccumulatedAfter;
}

public class DepreciationPreviewDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<DepreciationLineDto> Lines { get; set; } = new();
}

public class DepreciationRunResponseDto
{
    public Guid Id { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public DepreciationRunStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? JournalEntryId { get; set; }
    public string? JournalEntryEntryNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ReversedAt { get; set; }
    public string? ReversedBy { get; set; }
    public int AssetCount => Lines.Count;
    public List<DepreciationLineDto> Lines { get; set; } = new();
}
