using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Services;

public class ProjectFinancialService : IProjectFinancialService
{
    private readonly AppDbContext _db;

    public ProjectFinancialService(AppDbContext db) => _db = db;

    public async Task<ProjectFinancialSummaryDto> GetSummaryAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _db.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        var budgets = await _db.Set<ProjectBudget>()
            .Where(x => !x.IsDeleted && x.ProjectId == projectId)
            .ToListAsync(ct);

        // No budget rows is the common case early in a project's life, so the contract value
        // stands in as the ceiling rather than leaving the project with none.
        var budgetCeiling = budgets.Sum(b => b.RevisedAmount ?? b.AllocatedAmount);
        var hasBudget = budgets.Count > 0 && budgetCeiling > 0;
        var costCeiling = hasBudget ? budgetCeiling : project.ContractValue;

        var actualSpend = await GetActualSpendAsync(projectId, ct);
        var spendByAccount = await GetSpendByAccountAsync(projectId, ct);

        var raBills = await _db.Set<RABill>()
            .Where(x => !x.IsDeleted && x.ProjectId == projectId)
            .Select(x => new { x.Status, x.NetAmount })
            .ToListAsync(ct);

        var certified = raBills
            .Where(x => x.Status is RABillStatus.Certified or RABillStatus.Paid)
            .Sum(x => x.NetAmount);
        var pending = raBills
            .Where(x => x.Status is RABillStatus.Submitted or RABillStatus.UnderReview)
            .Sum(x => x.NetAmount);

        var margin = certified - actualSpend;

        return new ProjectFinancialSummaryDto
        {
            ProjectId = project.Id,
            ProjectCode = project.ProjectCode,
            ProjectName = project.ProjectName,
            ContractValue = project.ContractValue,

            RABilledCertified = certified,
            RABilledPending = pending,
            RemainingToBill = project.ContractValue - certified,
            ContractUtilizationPercent = Percent(certified, project.ContractValue),
            IsOverContractValue = certified > project.ContractValue,

            CostCeiling = costCeiling,
            CeilingSource = hasBudget ? CostCeilingSource.Budget : CostCeilingSource.ContractValue,
            ActualSpend = actualSpend,
            RemainingBudget = costCeiling - actualSpend,
            BudgetUtilizationPercent = Percent(actualSpend, costCeiling),
            IsOverBudget = actualSpend > costCeiling,

            Margin = margin,
            MarginPercent = Percent(margin, certified),

            SpendByAccount = spendByAccount
        };
    }

    public async Task<decimal> GetActualSpendAsync(Guid projectId, CancellationToken ct = default)
    {
        var costCenterIds = await GetCostCenterIdsAsync(projectId, ct);
        if (costCenterIds.Count == 0) return 0;

        return await SpendLines(costCenterIds)
            .SumAsync(l => l.DebitAmount - l.CreditAmount, ct);
    }

    private async Task<List<ProjectSpendByAccountDto>> GetSpendByAccountAsync(Guid projectId, CancellationToken ct)
    {
        var costCenterIds = await GetCostCenterIdsAsync(projectId, ct);
        if (costCenterIds.Count == 0) return new List<ProjectSpendByAccountDto>();

        var rows = await SpendLines(costCenterIds)
            .GroupBy(l => new { l.AccountId, l.Account.AccountCode, l.Account.AccountName })
            .Select(g => new ProjectSpendByAccountDto
            {
                AccountId = g.Key.AccountId,
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.AccountName,
                Amount = g.Sum(l => l.DebitAmount - l.CreditAmount)
            })
            .ToListAsync(ct);

        return rows.Where(r => r.Amount != 0).OrderByDescending(r => r.Amount).ToList();
    }

    /// <summary>
    /// A project can carry more than one cost center — one is auto-created with the project
    /// (ProjectCommands), and others may be added by hand.
    /// </summary>
    private async Task<List<Guid>> GetCostCenterIdsAsync(Guid projectId, CancellationToken ct)
        => await _db.Set<CostCenter>()
            .Where(c => !c.IsDeleted && c.ProjectId == projectId)
            .Select(c => c.Id)
            .ToListAsync(ct);

    /// <summary>
    /// Posted expense-account lines only. Restricting to Expense accounts is what stops a bill and
    /// its later payment being counted twice: the payment entry moves AP against Bank and touches
    /// no expense account.
    /// </summary>
    private IQueryable<JournalEntryLine> SpendLines(List<Guid> costCenterIds)
        => _db.Set<JournalEntryLine>()
            .Where(l => !l.IsDeleted
                && l.CostCenterId != null
                && costCenterIds.Contains(l.CostCenterId.Value)
                && l.JournalEntry.Status == JournalEntryStatus.Posted
                && l.Account.AccountType == AccountGroupType.Expense);

    private static decimal Percent(decimal amount, decimal total)
        => total == 0 ? 0 : Math.Round(amount / total * 100, 2);
}
