using uOrgHub.Projects.DTOs;

namespace uOrgHub.Projects.Services;

public interface IProjectFinancialService
{
    Task<ProjectFinancialSummaryDto> GetSummaryAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Actual spend for a project, from posted journal entry lines on its cost centers.
    /// Used by the project dashboard and budget summary in place of the never-updated
    /// ProjectBudget.SpentAmount column.
    /// </summary>
    Task<decimal> GetActualSpendAsync(Guid projectId, CancellationToken ct = default);
}
