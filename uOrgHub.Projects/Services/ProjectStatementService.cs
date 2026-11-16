using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;

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
        ProjectStatementTransactionQuery? transactions = null,
        CancellationToken ct = default)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            throw new AppException("Start date cannot be after end date.");

        // No query means the full ledger, unfiltered — the report screen asks for that and refines
        // it in memory. A query (the PDF) gets exactly the rows it describes.
        var fullLedger = transactions is null;
        transactions ??= new ProjectStatementTransactionQuery();

        // Model binding accepts any integer for an enum, so an undefined value must be refused
        // here rather than silently treated as one of the real filters.
        if (!Enum.IsDefined(transactions.TransactionType))
            throw new AppException("Transaction type must be All, Receipts or Payments.");
        if (transactions.Side.HasValue && !Enum.IsDefined(transactions.Side.Value))
            throw new AppException("Side must be Debit or Credit.");

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

        var rows = await LoadRowsAsync(projectId, costCenterIds, dateFrom, dateTo, ct);

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
            fullLedger ? rows : FilterRows(rows, transactions));
    }

    /// <summary>
    /// Narrows and orders the transaction list the way the report screen does, so a printout or
    /// PDF shows the rows the user filtered to. Applied last, after every summary figure and the
    /// account breakdown are computed from the full ledger, so a filter can never move a total.
    /// The running net is recomputed over the rows left visible by the reversed / cash-bank
    /// toggles, in posting order, before any other filter — so it reads as the project's cost to
    /// date in the view the user chose, and a search or party filter never changes it.
    ///
    /// Mirrors the screen's in-memory filter in ProjectStatementPage.tsx — a change to one must
    /// be made to the other, or the PDF will stop matching what the user saw.
    /// </summary>
    private static List<ProjectStatementRowDto> FilterRows(
        List<ProjectStatementRowDto> rows, ProjectStatementTransactionQuery q)
    {
        var runningNet = 0m;
        IEnumerable<ProjectStatementRowDto> result = rows
            .Where(r => (q.IncludeReversed || !r.IsReversed) && (q.IncludeCashBank || !r.IsCashBank))
            .Select(r => r with { RunningNet = runningNet += r.Debit - r.Credit })
            .ToList();

        result = q.TransactionType switch
        {
            ProjectStatementTransactionFilter.Receipts => result.Where(r => r.CashFlow == ProjectCashFlowType.Receipt),
            ProjectStatementTransactionFilter.Payments => result.Where(r => r.CashFlow == ProjectCashFlowType.Payment),
            _ => result,
        };

        var search = q.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            result = result.Where(r => string.Join(" ",
                    r.EntryNumber, r.ReferenceNumber, r.AccountCode, r.AccountName,
                    r.Narration, r.PaidTo, r.ReceivedFrom, r.CostCenterName)
                .Contains(search, StringComparison.OrdinalIgnoreCase));

        if (q.AccountId.HasValue)
            result = result.Where(r => r.AccountId == q.AccountId.Value);

        result = q.Side switch
        {
            ProjectStatementLineSide.Debit => result.Where(r => r.Debit > 0),
            ProjectStatementLineSide.Credit => result.Where(r => r.Credit > 0),
            _ => result,
        };

        if (!string.IsNullOrWhiteSpace(q.PaidTo))
            result = result.Where(r => PartyMatches(r.PaidTo, q.PaidTo));
        if (!string.IsNullOrWhiteSpace(q.ReceivedFrom))
            result = result.Where(r => PartyMatches(r.ReceivedFrom, q.ReceivedFrom));

        // A stable sort, so rows tied on the chosen column stay in posting order — the same as
        // the screen's. No sort (or an unknown column) leaves posting order as it is.
        return result.AsQueryable().ApplySorting(q.SortBy, q.SortDescending).ToList();
    }

    private static bool PartyMatches(string? party, string wanted)
        => string.Equals(party?.Trim(), wanted.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Every project's headline figures for the range in one pass. The ledger lines are not
    /// returned — across all projects that list has no natural bound — but each project's
    /// summary is computed on the same posted-line basis as <see cref="GetStatementAsync"/>, so
    /// a row here equals that project's own statement.
    /// </summary>
    public async Task<ConsolidatedProjectStatementDto> GetConsolidatedStatementAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            throw new AppException("Start date cannot be after end date.");

        var projects = await _db.Set<Project>()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.ProjectCode)
            .Select(p => new { p.Id, p.ProjectCode, p.ProjectName, p.ContractValue })
            .ToListAsync(ct);

        var costCenters = await _db.Set<CostCenter>()
            .Where(c => !c.IsDeleted && c.ProjectId != null)
            .Select(c => new { c.Id, ProjectId = c.ProjectId!.Value })
            .ToListAsync(ct);

        var ccIdsByProject = costCenters
            .GroupBy(c => c.ProjectId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());
        var allCcIds = costCenters.Select(c => c.Id).ToList();

        // Period movement per cost center, split by account type — one grouped query rather than
        // one per project, then attributed back to projects in memory.
        var periodQuery = PostedLines(allCcIds);
        if (dateFrom.HasValue)
            periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate >= dateFrom.Value);
        if (dateTo.HasValue)
            periodQuery = periodQuery.Where(l => l.JournalEntry.EntryDate <= dateTo.Value);

        var periodByCc = await periodQuery
            .GroupBy(l => new { CostCenterId = l.CostCenterId!.Value, l.Account.AccountType })
            .Select(g => new
            {
                g.Key.CostCenterId,
                g.Key.AccountType,
                Debit = g.Sum(x => x.DebitAmount),
                Credit = g.Sum(x => x.CreditAmount),
            })
            .ToListAsync(ct);

        // Opening spend per cost center — expense booked before the window. Nothing precedes an
        // open lower bound, so the query only runs when a start date is given.
        var openingByCc = new Dictionary<Guid, decimal>();
        if (dateFrom.HasValue)
        {
            var opening = await PostedLines(allCcIds)
                .Where(l => l.JournalEntry.EntryDate < dateFrom.Value)
                .Where(l => l.Account.AccountType == AccountGroupType.Expense)
                .GroupBy(l => l.CostCenterId!.Value)
                .Select(g => new { CostCenterId = g.Key, Net = g.Sum(x => x.DebitAmount - x.CreditAmount) })
                .ToListAsync(ct);
            openingByCc = opening.ToDictionary(x => x.CostCenterId, x => x.Net);
        }

        // Voucher cash flow per project — same basis and contra exclusion as GetVoucherCashFlowAsync.
        var voucherQuery = _db.Set<Voucher>()
            .Where(v => !v.IsDeleted
                && v.ProjectId != null
                && v.Status == VoucherStatus.Posted
                && (v.VoucherType == VoucherType.Credit || v.VoucherType == VoucherType.Debit));
        if (dateFrom.HasValue)
            voucherQuery = voucherQuery.Where(v => v.VoucherDate >= dateFrom.Value);
        if (dateTo.HasValue)
            voucherQuery = voucherQuery.Where(v => v.VoucherDate <= dateTo.Value);

        var voucherByProject = await voucherQuery
            .GroupBy(v => new { ProjectId = v.ProjectId!.Value, v.VoucherType })
            .Select(g => new { g.Key.ProjectId, g.Key.VoucherType, Total = g.Sum(v => v.Amount) })
            .ToListAsync(ct);

        var rows = new List<ConsolidatedProjectRowDto>(projects.Count);
        foreach (var p in projects)
        {
            var ccSet = ccIdsByProject.TryGetValue(p.Id, out var ids)
                ? ids.ToHashSet()
                : new HashSet<Guid>();

            var openingSpend = ccSet.Sum(id => openingByCc.TryGetValue(id, out var v) ? v : 0m);

            var period = periodByCc.Where(x => ccSet.Contains(x.CostCenterId)).ToList();
            var periodExpense = period
                .Where(x => x.AccountType == AccountGroupType.Expense)
                .Sum(x => x.Debit - x.Credit);
            var periodIncome = period
                .Where(x => x.AccountType == AccountGroupType.Income)
                .Sum(x => x.Credit - x.Debit);

            var receipts = voucherByProject
                .Where(v => v.ProjectId == p.Id && v.VoucherType == VoucherType.Credit)
                .Sum(v => v.Total);
            var payments = voucherByProject
                .Where(v => v.ProjectId == p.Id && v.VoucherType == VoucherType.Debit)
                .Sum(v => v.Total);

            rows.Add(new ConsolidatedProjectRowDto(
                p.Id, p.ProjectCode, p.ProjectName, p.ContractValue,
                openingSpend, periodExpense, periodIncome, openingSpend + periodExpense,
                receipts, payments, receipts - payments));
        }

        return new ConsolidatedProjectStatementDto(
            dateFrom,
            dateTo,
            rows.Sum(r => r.ContractValue),
            rows.Sum(r => r.OpeningSpend),
            rows.Sum(r => r.PeriodExpense),
            rows.Sum(r => r.PeriodIncome),
            rows.Sum(r => r.ClosingSpend),
            rows.Sum(r => r.Receipts),
            rows.Sum(r => r.Payments),
            rows.Sum(r => r.NetCashPosition),
            rows);
    }

    /// <summary>
    /// The ledger lines themselves, oldest first, with a running net (debit less credit) carried
    /// down the list — for a project that reads as cost accumulated so far within the period.
    /// </summary>
    private async Task<List<ProjectStatementRowDto>> LoadRowsAsync(
        Guid projectId,
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
                l.JournalEntryId,
                l.JournalEntry.EntryDate,
                l.JournalEntry.EntryNumber,
                l.JournalEntry.ReferenceNumber,
                l.AccountId,
                l.Account.AccountCode,
                l.Account.AccountName,
                l.Account.AccountType,
                l.Account.IsCashOrBank,
                l.Description,
                CostCenterName = l.CostCenter!.Name,
                l.DebitAmount,
                l.CreditAmount
            })
            .ToListAsync(ct);

        var vouchers = await LoadVoucherLinksAsync(
            projectId, lines.Select(l => l.JournalEntryId).Distinct().ToList(), ct);

        var rows = new List<ProjectStatementRowDto>(lines.Count);
        var runningNet = 0m;

        foreach (var line in lines)
        {
            runningNet += line.DebitAmount - line.CreditAmount;
            var hasVoucher = vouchers.TryGetValue(line.JournalEntryId, out var voucher);
            var isCashBank = line.IsCashOrBank
                || (hasVoucher && voucher.MoneyAccountIds.Contains(line.AccountId));
            rows.Add(new ProjectStatementRowDto(
                line.EntryDate,
                line.EntryNumber,
                line.ReferenceNumber,
                line.AccountId,
                line.AccountCode,
                line.AccountName,
                line.AccountType,
                line.Description,
                hasVoucher && voucher.Type == VoucherType.Debit ? voucher.Party : null,
                hasVoucher && voucher.Type == VoucherType.Credit ? voucher.Party : null,
                hasVoucher && voucher.ChargedToProject && voucher.Type != VoucherType.Contra
                    ? voucher.Type == VoucherType.Credit ? ProjectCashFlowType.Receipt : ProjectCashFlowType.Payment
                    : null,
                line.CostCenterName,
                line.DebitAmount,
                line.CreditAmount,
                runningNet,
                hasVoucher && voucher.CancelledPair,
                isCashBank));
        }

        return rows;
    }

    /// <summary>
    /// The voucher behind each journal entry, keyed by entry: its direction, the party it names,
    /// whether it is charged to this project — only those count as the project's receipts or
    /// payments, matching how <see cref="GetVoucherCashFlowAsync"/> sums them — which of its
    /// accounts is the money side, and whether it was reversed within the period.
    ///
    /// A voucher that was later corrected is matched on its reversal entry too. When both the
    /// original and the reversal are in the period the pair is flagged as cancelled: it nets to
    /// zero, the same as the summary, which drops a reversed voucher entirely. With only one of the
    /// two in the period, that entry is a real movement for the period and stays unflagged.
    ///
    /// The money side is read from the voucher, not from the account's cash-or-bank flag, which is
    /// not reliably set: a payment pays out of its credit account, a receipt lands in its debit
    /// account, and a contra moves money between two money accounts. A reversal swaps the sides
    /// but not the accounts, so the same rule finds the money line of the reversal entry.
    ///
    /// Contra vouchers name no party — their name field holds a cheque/slip number — and are
    /// neither a receipt nor a payment.
    /// </summary>
    private async Task<Dictionary<Guid, VoucherLink>> LoadVoucherLinksAsync(
        Guid projectId,
        List<Guid> entryIds,
        CancellationToken ct)
    {
        var vouchers = await _db.Set<Voucher>()
            .Where(v => !v.IsDeleted
                && ((v.JournalEntryId != null && entryIds.Contains(v.JournalEntryId.Value))
                    || (v.ReversalJournalEntryId != null && entryIds.Contains(v.ReversalJournalEntryId.Value))))
            .Select(v => new
            {
                v.JournalEntryId, v.ReversalJournalEntryId, v.VoucherType, v.Name, v.ProjectId,
                v.DebitAccountId, v.CreditAccountId,
            })
            .ToListAsync(ct);

        var inPeriod = entryIds.ToHashSet();
        var links = new Dictionary<Guid, VoucherLink>();
        foreach (var v in vouchers)
        {
            var party = v.VoucherType == VoucherType.Contra || string.IsNullOrWhiteSpace(v.Name) ? null : v.Name.Trim();
            Guid[] moneyAccounts = v.VoucherType switch
            {
                VoucherType.Debit => [v.CreditAccountId],
                VoucherType.Credit => [v.DebitAccountId],
                _ => [v.DebitAccountId, v.CreditAccountId],
            };
            var cancelledPair = v.JournalEntryId.HasValue && v.ReversalJournalEntryId.HasValue
                && inPeriod.Contains(v.JournalEntryId.Value) && inPeriod.Contains(v.ReversalJournalEntryId.Value);

            var link = new VoucherLink(v.VoucherType, party, v.ProjectId == projectId, moneyAccounts, cancelledPair);
            if (v.JournalEntryId.HasValue) links[v.JournalEntryId.Value] = link;
            if (v.ReversalJournalEntryId.HasValue) links[v.ReversalJournalEntryId.Value] = link;
        }
        return links;
    }

    private sealed record VoucherLink(
        VoucherType Type, string? Party, bool ChargedToProject, Guid[] MoneyAccountIds, bool CancelledPair);

    /// <summary>
    /// Grouped in memory rather than by a second query: the rows are already loaded, and the row
    /// count for one project over one period is small enough that a round trip would cost more
    /// than the grouping.
    ///
    /// Reversed pairs and cash/bank lines are left out. Neither moves an account's net — a pair
    /// cancels within its account, and the money side is not a project cost or income account —
    /// but both inflate the debit and credit columns, which users add up and compare with the
    /// summary band.
    /// </summary>
    private static List<ProjectStatementAccountDto> BuildAccountBreakdown(List<ProjectStatementRowDto> rows)
        => rows
            .Where(r => !r.IsReversed && !r.IsCashBank)
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
