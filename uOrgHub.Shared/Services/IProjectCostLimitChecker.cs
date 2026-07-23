namespace uOrgHub.Shared.Services;

/// <summary>An amount about to be charged to a cost center.</summary>
public readonly record struct ProjectCostAllocation(Guid CostCenterId, decimal Amount);

/// <summary>
/// Lets the Accounts module warn about project cost overruns without referencing the Projects
/// module (uOrgHub.Projects references uOrgHub.Accounts, not the other way round). Implemented in
/// uOrgHub.Projects and registered by AddProjectsModule.
/// </summary>
public interface IProjectCostLimitChecker
{
    /// <summary>
    /// Returns a human-readable warning when the allocations would push a project past its cost
    /// ceiling, or null when everything stays within. Amounts are grouped by the project behind
    /// each cost center, so a document spanning several projects is judged per project. Never
    /// throws for an overrun — overruns are reported, not blocked.
    /// </summary>
    Task<string?> CheckAsync(
        IEnumerable<ProjectCostAllocation> allocations,
        CancellationToken ct = default);
}
