using uOrgHub.Projects.DTOs.Reports;

namespace uOrgHub.Projects.Services;

public interface IProjectStatementService
{
    /// <summary>
    /// The project's accounting activity between the two dates. Both bounds are optional — an
    /// open range reports the project's whole life to date.
    /// </summary>
    Task<ProjectStatementDto> GetStatementAsync(
        Guid projectId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default);

    /// <summary>
    /// The same statement rolled up across every project for the date range: organisation-wide
    /// totals over a per-project breakdown, without the (unbounded) transaction ledger. Both
    /// bounds are optional, with the same open-range meaning as the single-project statement.
    /// </summary>
    Task<ConsolidatedProjectStatementDto> GetConsolidatedStatementAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default);
}
