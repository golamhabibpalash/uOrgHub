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
}
