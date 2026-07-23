using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Services;

namespace uOrgHub.Projects.Services;

public class ProjectCostLimitChecker : IProjectCostLimitChecker
{
    private readonly AppDbContext _db;
    private readonly IProjectFinancialService _financials;

    public ProjectCostLimitChecker(AppDbContext db, IProjectFinancialService financials)
    {
        _db = db;
        _financials = financials;
    }

    public async Task<string?> CheckAsync(
        IEnumerable<ProjectCostAllocation> allocations,
        CancellationToken ct = default)
    {
        var charged = allocations
            .Where(a => a.Amount > 0)
            .GroupBy(a => a.CostCenterId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        if (charged.Count == 0) return null;

        var costCenterIds = charged.Keys.ToList();

        var projectByCostCenter = await _db.Set<CostCenter>()
            .Where(c => !c.IsDeleted && costCenterIds.Contains(c.Id) && c.ProjectId != null)
            .Select(c => new { c.Id, ProjectId = c.ProjectId!.Value })
            .ToListAsync(ct);

        if (projectByCostCenter.Count == 0) return null;

        var amountByProject = projectByCostCenter
            .GroupBy(x => x.ProjectId)
            .ToDictionary(g => g.Key, g => g.Sum(x => charged[x.Id]));

        var warnings = new List<string>();

        foreach (var (projectId, additional) in amountByProject)
        {
            var summary = await _financials.GetSummaryAsync(projectId, ct);
            if (summary.CostCeiling <= 0) continue;

            // This document has not posted yet, so add it to what is already on the books.
            var projected = summary.ActualSpend + additional;
            if (projected <= summary.CostCeiling) continue;

            var ceilingLabel = summary.CeilingSource == CostCeilingSource.Budget
                ? "budget"
                : "contract value";

            warnings.Add(
                $"Project {summary.ProjectCode} goes over {ceilingLabel} by "
                + $"{projected - summary.CostCeiling:N2} ({projected:N2} of {summary.CostCeiling:N2}).");
        }

        return warnings.Count == 0 ? null : string.Join(" ", warnings);
    }
}
