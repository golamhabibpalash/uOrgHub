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
    UnderMaintenance,
    /// <summary>
    /// On a project site. Set and cleared only by deploying and returning the asset — never by
    /// editing it — so the status always agrees with the deployment record.
    /// </summary>
    Deployed
}

public enum DepreciationRunStatus
{
    Posted,
    Reversed
}

/// <summary>How a project pays for the use of company equipment while it is deployed there.</summary>
public enum DeploymentChargeMode
{
    /// <summary>The project is charged an internal hire rate for each day on site.</summary>
    HireRate,
    /// <summary>No hire charge; the project carries only fuel, operator and repairs, booked as project expenses.</summary>
    RunningCostsOnly,
    /// <summary>All equipment cost stays at company level.</summary>
    None
}

public enum HireRateUnit
{
    PerDay,
    /// <summary>Prorated by day: each day on site costs the rate ÷ the days in that calendar month.</summary>
    PerMonth
}

public enum HireChargeRunStatus
{
    Posted,
    Reversed
}
