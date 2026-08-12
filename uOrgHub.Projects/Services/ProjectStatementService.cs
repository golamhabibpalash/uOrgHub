using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Services;

/// <summary>
/// Builds the project-wise accounting statement.
///
/// Lives in uOrgHub.Projects rather than uOrgHub.Accounts because it needs both sides — the
/// project's identity and contract value, and the general ledger it is charged against — and the
/// reference only runs one way: Projects sees Accounts, never the reverse.
///
/// The link between the two is the cost center. A project owns one or more cost centers, and a
/// journal entry line carrying one of them is charged to that project. This is the same basis
/// <see cref="ProjectFinancialService"/> works on, deliberately: the statement is the itemised
/// form of the summary that service returns, and the two must reconcile.
/// </summary>
public class ProjectStatementService : IProjectStatementService
{
    private readonly AppDbContext _db;

    public ProjectStatementService(AppDbContext db) => _db = db;

    public async Task<ProjectStatementDto> GetStatementAsync(
        Guid projectId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            throw new AppException("Start date cannot be after end date.");

        var project = await _db.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        var costCenterIds = await _db.Set<CostCenter>()
            .Where(c => !c.IsDeleted && c.ProjectId == projectId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        // A project with no cost center has nothing charged to it yet. That is an ordinary state
        // early on, not an error — return an empty statement rather than failing the report.
        if (costCenterIds.Count == 0)
            return EmptyStatement(project, dateFrom, dateTo);

        var rows = await LoadRowsAsync(costCenterIds, dateFrom, dateTo, ct);

        // Opening is everything before the window, so the period figures read as a movement
        // rather than a running total. With no lower bound there is nothing before it.
        var openingSpend = dateFrom.HasValue
            ? await PostedLines(costCenterIds)
                .Where(l => l.JournalEntry.EntryDate < dateFrom.Value)
                .Where(l => l.Account.AccountType == AccountGroupType.Expense)
                .SumAsync(l => l.DebitAmount - l.CreditAmount, ct)
            : 0m;

        var periodExpense = rows
            .Where(r => r.AccountType == AccountGroupType.Expense)
            .Sum(r => r.Debit - r.Credit);

        // Income is credit-normal, so its sign is the mirror of expense.
        var periodIncome = rows
            .Where(r => r.AccountType == AccountGroupType.Income)
            .Sum(r => r.Credit - r.Debit);

        var (receipts, payments) = await GetVoucherCashFlowAsync(projectId, dateFrom, dateTo, ct);

        return new ProjectStatementDto(
            project.Id,
            project.ProjectCode,
            project.ProjectName,
            project.ContractValue,
            dateFrom,
            dateTo,
            openingSpend,
            periodExpense,
            periodIncome,
            openingSpend + periodExpense,
            receipts,
            payments,
            receipts - payments,
            BuildAccountBreakdown(rows),
            rows);
    }

    /// <summary>
    /// The ledger lines themselves, oldest first, with a running net (debit less credit) carried
    /// down the list — for a project that reads as cost accumulated so far within the period.
    /// </summary>
    private async Task<List<ProjectStatementRowDto>> LoadRowsAsync(
        List<Guid> costCenterIds,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct)
    {
        var query = PostedLines(costCenterIds);

        if (dateFrom.HasValue)
            query = query.Where(l => l.JournalEntry.EntryDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(l => l.JournalEntry.EntryDate <= dateTo.Value);

        var lines = await query
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.JournalEntry.EntryNumber)
            .ThenBy(l => l.LineOrder)
            .Select(l => new
            {
                l.JournalEntry.EntryDate,
                l.JournalEntry.EntryNumber,
                l.JournalEntry.ReferenceNumber,
                l.AccountId,
                l.Account.AccountCode,
                l.Account.AccountName,
                l.Account.AccountType,
                l.Description,
                CostCenterName = l.CostCenter!.Name,
                l.DebitAmount,
                l.CreditAmount
            })
            .ToListAsync(ct);

        var rows = new List<ProjectStatementRowDto>(lines.Count);
        var runningNet = 0m;

        foreach (var line in lines)
        {
            runningNet += line.DebitAmount - line.CreditAmount;
            rows.Add(new ProjectStatementRowDto(
                line.EntryDate,
                line.EntryNumber,
                line.ReferenceNumber,
                line.AccountId,
                line.AccountCode,
                line.AccountName,
                line.AccountType,
                line.Description,
                line.CostCenterName,
                line.DebitAmount,
                line.CreditAmount,
                runningNet));
        }

        return rows;
    }

    /// <summary>
    /// Grouped in memory rather than by a second query: the rows are already loaded, and the row
    /// count for one project over one period is small enough that a round trip would cost more
    /// than the grouping.
    /// </summary>
    private static List<ProjectStatementAccountDto> BuildAccountBreakdown(List<ProjectStatementRowDto> rows)
        => rows
            .GroupBy(r => new { r.AccountId, r.AccountCode, r.AccountName, r.AccountType })
            .Select(g => new ProjectStatementAccountDto(
                g.Key.AccountId,
                g.Key.AccountCode,
                g.Key.AccountName,
                g.Key.AccountType,
                g.Sum(r => r.Debit),
                g.Sum(r => r.Credit),
                g.Sum(r => r.Debit - r.Credit)))
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .ToList();

    /// <summary>
    /// Cash in and out for the project from posted vouchers dated in the period. The voucher type
    /// states the direction, so no account-type reasoning is needed.
    ///
    /// Contra vouchers are excluded: they move money between the organisation's own accounts, so
    /// counting one would inflate the project's cash flow with money that never entered or left
    /// it. Same exclusion as ProjectFinancialService, for the same reason.
    /// </summary>
    private async Task<(decimal Receipts, decimal Payments)> GetVoucherCashFlowAsync(
        Guid projectId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct)
    {
        var query = _db.Set<Voucher>()
            .Where(v => !v.IsDeleted
                && v.ProjectId == projectId
                && v.Status == VoucherStatus.Posted
                && (v.VoucherType == VoucherType.Credit || v.VoucherType == VoucherType.Debit));

        if (dateFrom.HasValue)
            query = query.Where(v => v.VoucherDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(v => v.VoucherDate <= dateTo.Value);

        var rows = await query
            .GroupBy(v => v.VoucherType)
            .Select(g => new { VoucherType = g.Key, Total = g.Sum(v => v.Amount) })
            .ToListAsync(ct);

        return (
            rows.Where(r => r.VoucherType == VoucherType.Credit).Sum(r => r.Total),
            rows.Where(r => r.VoucherType == VoucherType.Debit).Sum(r => r.Total));
    }

    private IQueryable<JournalEntryLine> PostedLines(List<Guid> costCenterIds)
        => _db.Set<JournalEntryLine>()
            .Where(l => !l.IsDeleted
                && l.CostCenterId != null
                && costCenterIds.Contains(l.CostCenterId.Value)
                && l.JournalEntry.Status == JournalEntryStatus.Posted);

    private static ProjectStatementDto EmptyStatement(Project project, DateTime? dateFrom, DateTime? dateTo)
        => new(
            project.Id, project.ProjectCode, project.ProjectName, project.ContractValue,
            dateFrom, dateTo,
            0m, 0m, 0m, 0m, 0m, 0m, 0m,
            new List<ProjectStatementAccountDto>(),
            new List<ProjectStatementRowDto>());
}
