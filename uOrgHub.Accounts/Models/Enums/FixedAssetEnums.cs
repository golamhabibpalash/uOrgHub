namespace uOrgHub.Accounts.Models.Enums;

public enum DepreciationMethod
{
    /// <summary>(Cost − salvage) spread evenly over the useful life.</summary>
    StraightLine,
    /// <summary>Double-declining balance: 2 ÷ life (in months) of the current book value each month.</summary>
    DecliningBalance
}

/// <summary>
/// Operational state only. Depreciation keeps running in every state — an idle or broken-down
/// machine still loses value — so none of these pause the monthly run.
/// </summary>
public enum FixedAssetStatus
{
    Active,
    Idle,
    UnderMaintenance
}

public enum DepreciationRunStatus
{
    Posted,
    Reversed
}
