using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Services;

public class AccountingReportService : IAccountingReportService
{
    private readonly AppDbContext _db;

    public AccountingReportService(AppDbContext db) => _db = db;

    public async Task<TrialBalanceResponseDto> GetTrialBalanceAsync(ReportFilterDto filter)
    {
        var query = _db.Set<ChartOfAccount>()
            .Include(a => a.AccountGroup)
            .Where(a => !a.IsDeleted && a.IsActive);

        if (filter.AccountType.HasValue)
            query = query.Where(a => a.AccountType == filter.AccountType.Value);
        if (filter.AccountGroupId.HasValue)
            query = query.Where(a => a.AccountGroupId == filter.AccountGroupId.Value);
        if (filter.AccountId.HasValue)
            query = query.Where(a => a.Id == filter.AccountId.Value);

        var accounts = await query.OrderBy(a => a.AccountCode).ToListAsync();

        var rows = new List<TrialBalanceRowDto>();

        foreach (var account in accounts)
        {
            var periodDebit = 0m;
            var periodCredit = 0m;

            if (filter.DateFrom.HasValue || filter.DateTo.HasValue)
            {
                var lines = _db.Set<JournalEntryLine>()
                    .Include(l => l.JournalEntry)
                    .Where(l => l.AccountId == account.Id && l.JournalEntry.Status == JournalEntryStatus.Posted);

                if (filter.DateFrom.HasValue)
                    lines = lines.Where(l => l.JournalEntry.EntryDate >= filter.DateFrom.Value);
                if (filter.DateTo.HasValue)
                    lines = lines.Where(l => l.JournalEntry.EntryDate <= filter.DateTo.Value);

                var aggregates = await lines.GroupBy(l => 1)
                    .Select(g => new { Debit = g.Sum(l => l.DebitAmount), Credit = g.Sum(l => l.CreditAmount) })
                    .FirstOrDefaultAsync();

                periodDebit = aggregates?.Debit ?? 0;
                periodCredit = aggregates?.Credit ?? 0;
            }

            var isDebitNormal = account.AccountType is AccountGroupType.Asset or AccountGroupType.Expense;
            var netChange = isDebitNormal ? periodDebit - periodCredit : periodCredit - periodDebit;
            var openingBalance = account.CurrentBalance - netChange;

            var openingDebit = isDebitNormal ? Math.Max(0, openingBalance) : Math.Max(0, -openingBalance);
            var openingCredit = isDebitNormal ? Math.Max(0, -openingBalance) : Math.Max(0, openingBalance);
            var closingDebit = isDebitNormal ? Math.Max(0, account.CurrentBalance) : Math.Max(0, -account.CurrentBalance);
            var closingCredit = isDebitNormal ? Math.Max(0, -account.CurrentBalance) : Math.Max(0, account.CurrentBalance);

            rows.Add(new TrialBalanceRowDto(
                account.Id, account.AccountCode, account.AccountName,
                account.AccountGroup?.Name ?? "", account.AccountType,
                openingDebit, openingCredit,
                periodDebit, periodCredit,
                closingDebit, closingCredit
            ));
        }

        return new TrialBalanceResponseDto(
            rows,
            rows.Sum(r => r.OpeningDebit), rows.Sum(r => r.OpeningCredit),
            rows.Sum(r => r.Debit), rows.Sum(r => r.Credit),
            rows.Sum(r => r.ClosingDebit), rows.Sum(r => r.ClosingCredit)
        );
    }

    public async Task<List<GeneralLedgerRowDto>> GetGeneralLedgerAsync(ReportFilterDto filter)
    {
        var query = _db.Set<ChartOfAccount>()
            .Include(a => a.AccountGroup)
            .Where(a => !a.IsDeleted && a.IsActive);

        if (filter.AccountType.HasValue)
            query = query.Where(a => a.AccountType == filter.AccountType.Value);
        if (filter.AccountGroupId.HasValue)
            query = query.Where(a => a.AccountGroupId == filter.AccountGroupId.Value);
        if (filter.AccountId.HasValue)
            query = query.Where(a => a.Id == filter.AccountId.Value);

        var accounts = await query.OrderBy(a => a.AccountCode).ToListAsync();
        var rows = new List<GeneralLedgerRowDto>();

        foreach (var account in accounts)
        {
            var lines = _db.Set<JournalEntryLine>()
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id && l.JournalEntry.Status == JournalEntryStatus.Posted);

            if (filter.DateFrom.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate <= filter.DateTo.Value);

            var aggregates = await lines.GroupBy(l => 1)
                .Select(g => new { Debit = g.Sum(l => l.DebitAmount), Credit = g.Sum(l => l.CreditAmount) })
                .FirstOrDefaultAsync();

            var debit = aggregates?.Debit ?? 0;
            var credit = aggregates?.Credit ?? 0;

            var isDebitNormal = account.AccountType is AccountGroupType.Asset or AccountGroupType.Expense;
            var netChange = isDebitNormal ? debit - credit : credit - debit;
            var openingBalance = account.CurrentBalance - netChange;

            rows.Add(new GeneralLedgerRowDto(
                account.Id, account.AccountCode, account.AccountName,
                account.AccountGroup?.Name ?? "", account.AccountType,
                openingBalance, debit, credit, account.CurrentBalance
            ));
        }

        return rows;
    }

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(ReportFilterDto filter)
    {
        var accounts = await _db.Set<ChartOfAccount>()
            .Include(a => a.AccountGroup)
            .Where(a => !a.IsDeleted && a.IsActive)
            .Where(a => a.AccountType == AccountGroupType.Income || a.AccountType == AccountGroupType.Expense)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var revenueLines = new List<IncomeStatementLineDto>();
        var expenseLines = new List<IncomeStatementLineDto>();
        var totalRevenue = 0m;
        var totalExpenses = 0m;

        foreach (var account in accounts)
        {
            var lines = _db.Set<JournalEntryLine>()
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id && l.JournalEntry.Status == JournalEntryStatus.Posted);

            if (filter.DateFrom.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate <= filter.DateTo.Value);

            var aggregates = await lines.GroupBy(l => 1)
                .Select(g => new { Debit = g.Sum(l => l.DebitAmount), Credit = g.Sum(l => l.CreditAmount) })
                .FirstOrDefaultAsync();

            var balance = account.AccountType == AccountGroupType.Income
                ? (aggregates?.Credit ?? 0) - (aggregates?.Debit ?? 0)
                : (aggregates?.Debit ?? 0) - (aggregates?.Credit ?? 0);

            var line = new IncomeStatementLineDto(
                $"[{account.AccountCode}] {account.AccountName}", balance, false, null
            );

            if (account.AccountType == AccountGroupType.Income)
            {
                revenueLines.Add(line);
                totalRevenue += balance;
            }
            else
            {
                expenseLines.Add(line);
                totalExpenses += balance;
            }
        }

        var costOfSales = 0m;
        var grossProfit = totalRevenue - costOfSales;
        var netProfit = totalRevenue - totalExpenses;

        var allLines = new List<IncomeStatementLineDto>
        {
            new("Revenue", totalRevenue, true, revenueLines.Count > 0 ? revenueLines : null),
        };

        if (costOfSales != 0)
        {
            allLines.Add(new IncomeStatementLineDto("Cost of Sales", costOfSales, true, null));
            allLines.Add(new IncomeStatementLineDto("Gross Profit", grossProfit, true, null));
        }

        allLines.Add(new IncomeStatementLineDto("Expenses", totalExpenses, true, expenseLines.Count > 0 ? expenseLines : null));
        allLines.Add(new IncomeStatementLineDto("Net Profit / (Loss)", netProfit, true, null));

        return new IncomeStatementDto(totalRevenue, costOfSales, grossProfit, totalExpenses, netProfit, allLines);
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(ReportFilterDto filter)
    {
        var accounts = await _db.Set<ChartOfAccount>()
            .Include(a => a.AccountGroup)
            .Where(a => !a.IsDeleted && a.IsActive)
            .Where(a => a.AccountType == AccountGroupType.Asset || a.AccountType == AccountGroupType.Liability || a.AccountType == AccountGroupType.Equity)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var assetLines = new List<BalanceSheetLineDto>();
        var liabilityLines = new List<BalanceSheetLineDto>();
        var equityLines = new List<BalanceSheetLineDto>();
        var totalAssets = 0m;
        var totalLiabilities = 0m;
        var totalEquity = 0m;

        foreach (var account in accounts)
        {
            var lines = _db.Set<JournalEntryLine>()
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id && l.JournalEntry.Status == JournalEntryStatus.Posted);

            if (filter.DateFrom.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate <= filter.DateTo.Value);

            var aggregates = await lines.GroupBy(l => 1)
                .Select(g => new { Debit = g.Sum(l => l.DebitAmount), Credit = g.Sum(l => l.CreditAmount) })
                .FirstOrDefaultAsync();

            var balance = account.AccountType switch
            {
                AccountGroupType.Asset => (aggregates?.Debit ?? 0) - (aggregates?.Credit ?? 0),
                AccountGroupType.Liability => (aggregates?.Credit ?? 0) - (aggregates?.Debit ?? 0),
                AccountGroupType.Equity => (aggregates?.Credit ?? 0) - (aggregates?.Debit ?? 0),
                _ => 0,
            };

            if (balance == 0) balance = account.CurrentBalance;

            var line = new BalanceSheetLineDto(
                $"[{account.AccountCode}] {account.AccountName}", Math.Abs(balance), false, null
            );

            switch (account.AccountType)
            {
                case AccountGroupType.Asset:
                    assetLines.Add(line);
                    totalAssets += Math.Abs(balance);
                    break;
                case AccountGroupType.Liability:
                    liabilityLines.Add(line);
                    totalLiabilities += Math.Abs(balance);
                    break;
                case AccountGroupType.Equity:
                    equityLines.Add(line);
                    totalEquity += Math.Abs(balance);
                    break;
            }
        }

        var allLines = new List<BalanceSheetLineDto>
        {
            new("Assets", totalAssets, true, assetLines.Count > 0 ? assetLines : null),
            new("Liabilities", totalLiabilities, true, liabilityLines.Count > 0 ? liabilityLines : null),
            new("Equity", totalEquity, true, equityLines.Count > 0 ? equityLines : null),
        };

        return new BalanceSheetDto(totalAssets, totalLiabilities, totalEquity, allLines);
    }

    public async Task<List<AccountLedgerRowDto>> GetAccountLedgerAsync(Guid accountId, DateTime? dateFrom, DateTime? dateTo)
    {
        var lines = _db.Set<JournalEntryLine>()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId && l.JournalEntry.Status == JournalEntryStatus.Posted);

        if (dateFrom.HasValue)
            lines = lines.Where(l => l.JournalEntry.EntryDate >= dateFrom.Value);
        if (dateTo.HasValue)
            lines = lines.Where(l => l.JournalEntry.EntryDate <= dateTo.Value);

        var entries = await lines
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.JournalEntry.EntryNumber)
            .Select(l => new
            {
                l.JournalEntry.EntryDate,
                l.JournalEntry.EntryNumber,
                l.JournalEntry.ReferenceNumber,
                l.Description,
                l.DebitAmount,
                l.CreditAmount,
            })
            .ToListAsync();

        var account = await _db.Set<ChartOfAccount>()
            .FirstOrDefaultAsync(a => a.Id == accountId);

        var runningBalance = account?.CurrentBalance ?? 0;
        var periodNet = 0m;
        var isDebitNormal = account?.AccountType is AccountGroupType.Asset or AccountGroupType.Expense;

        foreach (var e in entries)
        {
            periodNet += isDebitNormal ? e.DebitAmount - e.CreditAmount : e.CreditAmount - e.DebitAmount;
        }

        runningBalance = account?.CurrentBalance ?? 0;
        var bal = runningBalance - periodNet;

        var result = new List<AccountLedgerRowDto>();
        foreach (var e in entries)
        {
            bal += isDebitNormal ? e.DebitAmount - e.CreditAmount : e.CreditAmount - e.DebitAmount;
            result.Add(new AccountLedgerRowDto(
                e.EntryDate, e.EntryNumber, e.ReferenceNumber,
                e.Description ?? "", e.DebitAmount, e.CreditAmount, bal
            ));
        }

        return result;
    }

    public async Task<List<AccountLedgerGroupDto>> GetAllAccountsLedgerAsync(DateTime? dateFrom, DateTime? dateTo)
    {
        var accounts = await _db.Set<ChartOfAccount>()
            .Include(a => a.AccountGroup)
            .Where(a => !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var groups = new List<AccountLedgerGroupDto>();

        foreach (var account in accounts)
        {
            var lines = _db.Set<JournalEntryLine>()
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id && l.JournalEntry.Status == JournalEntryStatus.Posted);

            if (dateFrom.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate >= dateFrom.Value);
            if (dateTo.HasValue)
                lines = lines.Where(l => l.JournalEntry.EntryDate <= dateTo.Value);

            var entries = await lines
                .OrderBy(l => l.JournalEntry.EntryDate)
                .ThenBy(l => l.JournalEntry.EntryNumber)
                .Select(l => new
                {
                    l.JournalEntry.EntryDate,
                    l.JournalEntry.EntryNumber,
                    l.JournalEntry.ReferenceNumber,
                    l.Description,
                    l.DebitAmount,
                    l.CreditAmount,
                })
                .ToListAsync();

            if (entries.Count == 0)
                continue;

            var isDebitNormal = account.AccountType is AccountGroupType.Asset or AccountGroupType.Expense;

            var totalDebit = entries.Sum(e => e.DebitAmount);
            var totalCredit = entries.Sum(e => e.CreditAmount);
            var periodNet = isDebitNormal ? totalDebit - totalCredit : totalCredit - totalDebit;

            var closingBalance = account.CurrentBalance;
            var openingBalance = closingBalance - periodNet;

            var bal = openingBalance;
            var rows = new List<AccountLedgerRowDto>();
            foreach (var e in entries)
            {
                bal += isDebitNormal ? e.DebitAmount - e.CreditAmount : e.CreditAmount - e.DebitAmount;
                rows.Add(new AccountLedgerRowDto(
                    e.EntryDate, e.EntryNumber, e.ReferenceNumber,
                    e.Description ?? "", e.DebitAmount, e.CreditAmount, bal
                ));
            }

            groups.Add(new AccountLedgerGroupDto(
                account.Id, account.AccountCode, account.AccountName,
                account.AccountGroup?.Name ?? "", account.AccountType,
                openingBalance, closingBalance, totalDebit, totalCredit, rows
            ));
        }

        return groups;
    }

    public async Task<DayBookReportDto> GetDayBookAsync(DayBookFilterDto filter, PaginationRequest request)
    {
        var query = _db.Set<JournalEntry>()
            .Where(j => !j.IsDeleted);

        if (filter.DateFrom.HasValue)
            query = query.Where(j => j.EntryDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(j => j.EntryDate <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.WhereSearch(request.Search, j => j.EntryNumber, j => j.Description, j => j.ReferenceNumber!);

        // The day book classifies each entry by the voucher that generated it — DR (Debit),
        // CR (Credit), CN (Contra) — and everything else (bills, invoices, payments, hand-written
        // entries) is a Journal Voucher, JV.
        var type = filter.Type?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(type))
        {
            query = type switch
            {
                "DR" => query.Where(j => _db.Set<Voucher>().Any(v => !v.IsDeleted && v.JournalEntryId == j.Id && v.VoucherType == VoucherType.Debit)),
                "CR" => query.Where(j => _db.Set<Voucher>().Any(v => !v.IsDeleted && v.JournalEntryId == j.Id && v.VoucherType == VoucherType.Credit)),
                "CN" => query.Where(j => _db.Set<Voucher>().Any(v => !v.IsDeleted && v.JournalEntryId == j.Id && v.VoucherType == VoucherType.Contra)),
                "JV" => query.Where(j => !_db.Set<Voucher>().Any(v => !v.IsDeleted && v.JournalEntryId == j.Id)),
                _ => query,
            };
        }

        var totalCount = await query.CountAsync();

        var rows = await query
            .ApplySorting(request.SortBy ?? "EntryDate", request.SortDescending, j => j.EntryNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Voucher types are resolved once for the whole page — a joined subquery per row would
        // otherwise cost a round trip for every entry on the page.
        var entryIds = rows.Select(j => j.Id).ToList();
        var voucherTypes = await _db.Set<Voucher>()
            .Where(v => !v.IsDeleted && v.JournalEntryId != null && entryIds.Contains(v.JournalEntryId.Value))
            .Select(v => new { EntryId = v.JournalEntryId!.Value, v.VoucherType })
            .ToListAsync();

        var typeByEntry = voucherTypes
            .GroupBy(v => v.EntryId)
            .ToDictionary(g => g.Key, g => g.First().VoucherType);

        var items = rows.Select(j => new DayBookRowDto(
            j.Id,
            j.EntryDate,
            j.EntryNumber,
            j.ReferenceNumber,
            j.Description,
            j.Status.ToString(),
            typeByEntry.TryGetValue(j.Id, out var voucherType)
                ? voucherType switch
                {
                    VoucherType.Debit => "DR",
                    VoucherType.Credit => "CR",
                    _ => "CN",
                }
                : "JV",
            j.TotalDebit,
            j.TotalCredit,
            j.CreatedBy
        )).ToList();

        // Grand totals cover every filtered entry, not just the page, so the footer stays correct
        // as the user pages through the register. Resolved in a single aggregate over the query.
        var totals = await query
            .GroupBy(j => 1)
            .Select(g => new { Debit = g.Sum(j => j.TotalDebit), Credit = g.Sum(j => j.TotalCredit) })
            .FirstOrDefaultAsync();

        return new DayBookReportDto(
            new PagedResult<DayBookRowDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            },
            totals?.Debit ?? 0,
            totals?.Credit ?? 0
        );
    }

    public async Task<List<ChartOfAccountsReportRowDto>> GetChartOfAccountsReportAsync(ReportFilterDto filter)
    {
        var query = _db.Set<ChartOfAccount>()
            .Include(a => a.AccountGroup)
            .Where(a => !a.IsDeleted);

        if (filter.AccountType.HasValue)
            query = query.Where(a => a.AccountType == filter.AccountType.Value);
        if (filter.AccountGroupId.HasValue)
            query = query.Where(a => a.AccountGroupId == filter.AccountGroupId.Value);
        if (filter.AccountId.HasValue)
            query = query.Where(a => a.Id == filter.AccountId.Value);
        if (filter.Status is not null)
        {
            var active = filter.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(a => a.IsActive == active);
        }

        var accounts = await query
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new ChartOfAccountsReportRowDto(
                a.Id, a.AccountCode, a.AccountName,
                a.AccountGroup!.Name, a.AccountType,
                a.CurrentBalance, a.IsActive, a.CustomCode
            ))
            .ToListAsync();

        return accounts;
    }

    public async Task<PagedResult<JournalEntryReportRowDto>> GetJournalEntryReportAsync(ReportFilterDto filter, PaginationRequest request)
    {
        var query = _db.Set<JournalEntry>()
            .Where(j => !j.IsDeleted);

        if (filter.DateFrom.HasValue)
            query = query.Where(j => j.EntryDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(j => j.EntryDate <= filter.DateTo.Value);
        if (filter.Status is not null)
            query = query.Where(j => j.Status.ToString() == filter.Status);
        if (filter.CreatedBy is not null)
            query = query.Where(j => j.CreatedBy.Contains(filter.CreatedBy));

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.WhereSearch(request.Search, j => j.EntryNumber, j => j.Description, j => j.ReferenceNumber!);

        var totalCount = await query.CountAsync();

        var items = await query
            .ApplySorting(request.SortBy ?? "EntryDate", request.SortDescending, j => j.EntryNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new JournalEntryReportRowDto(
                j.Id, j.EntryNumber, j.EntryDate, j.ReferenceNumber,
                j.Description, j.TotalDebit, j.TotalCredit,
                j.Status.ToString(), j.CreatedBy, j.CreatedAt
            ))
            .ToListAsync();

        return new PagedResult<JournalEntryReportRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<AccountGroupSummaryRowDto>> GetAccountGroupSummaryAsync(ReportFilterDto filter)
    {
        var query = _db.Set<AccountGroup>()
            .Where(g => !g.IsDeleted);

        if (filter.AccountType.HasValue)
            query = query.Where(g => g.Type == filter.AccountType.Value);

        var groups = await query.OrderBy(g => g.Code).ToListAsync();
        var rows = new List<AccountGroupSummaryRowDto>();

        foreach (var group in groups)
        {
            var accounts = _db.Set<ChartOfAccount>()
                .Where(a => a.AccountGroupId == group.Id && !a.IsDeleted && a.IsActive);

            var accountList = await accounts.ToListAsync();
            var accountCount = accountList.Count;

            var totalDebit = 0m;
            var totalCredit = 0m;

            foreach (var account in accountList)
            {
                var isDebitNormal = account.AccountType is AccountGroupType.Asset or AccountGroupType.Expense;
                if (isDebitNormal)
                {
                    if (account.CurrentBalance > 0) totalDebit += account.CurrentBalance;
                    else totalCredit += Math.Abs(account.CurrentBalance);
                }
                else
                {
                    if (account.CurrentBalance > 0) totalCredit += account.CurrentBalance;
                    else totalDebit += Math.Abs(account.CurrentBalance);
                }
            }

            var balance = totalDebit - totalCredit;
            rows.Add(new AccountGroupSummaryRowDto(
                group.Id, group.Code, group.Name,
                group.Type, totalDebit, totalCredit, balance, accountCount
            ));
        }

        return rows;
    }

    public async Task<AgingSummaryDto> GetARAgingReportAsync(DateTime asOfDate)
    {
        var invoices = await _db.Set<Invoice>()
            .Include(i => i.Customer)
            .Where(i => !i.IsDeleted)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Void)
            .ToListAsync();

        var rows = invoices.Select(i =>
        {
            var balanceDue = i.TotalAmount - i.PaidAmount;
            var daysOverdue = (asOfDate.Date - i.DueDate.Date).Days;
            var bucket = daysOverdue switch
            {
                <= 0 => "Current",
                <= 30 => "1-30 Days",
                <= 60 => "31-60 Days",
                <= 90 => "61-90 Days",
                _ => "90+ Days"
            };

            return new AgingRowDto(
                i.Id, i.Customer.Name, i.InvoiceNumber,
                i.InvoiceDate, i.DueDate, i.TotalAmount, i.PaidAmount,
                balanceDue, Math.Max(0, daysOverdue), bucket
            );
        }).OrderByDescending(r => r.DaysOverdue).ToList();

        return BuildAgingSummary(rows);
    }

    public async Task<AgingSummaryDto> GetAPAgingReportAsync(DateTime asOfDate)
    {
        var bills = await _db.Set<Bill>()
            .Include(b => b.Vendor)
            .Where(b => !b.IsDeleted)
            .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled && b.Status != BillStatus.Void)
            .ToListAsync();

        var rows = bills.Select(b =>
        {
            var balanceDue = b.TotalAmount - b.PaidAmount;
            var daysOverdue = (asOfDate.Date - b.DueDate.Date).Days;
            var bucket = daysOverdue switch
            {
                <= 0 => "Current",
                <= 30 => "1-30 Days",
                <= 60 => "31-60 Days",
                <= 90 => "61-90 Days",
                _ => "90+ Days"
            };

            return new AgingRowDto(
                b.Id, b.Vendor.Name, b.BillNumber,
                b.BillDate, b.DueDate, b.TotalAmount, b.PaidAmount,
                balanceDue, Math.Max(0, daysOverdue), bucket
            );
        }).OrderByDescending(r => r.DaysOverdue).ToList();

        return BuildAgingSummary(rows);
    }

    private static AgingSummaryDto BuildAgingSummary(List<AgingRowDto> rows)
    {
        return new AgingSummaryDto(
            rows.Where(r => r.AgingBucket == "Current").Sum(r => r.BalanceDue),
            rows.Where(r => r.AgingBucket == "1-30 Days").Sum(r => r.BalanceDue),
            rows.Where(r => r.AgingBucket == "31-60 Days").Sum(r => r.BalanceDue),
            rows.Where(r => r.AgingBucket == "61-90 Days").Sum(r => r.BalanceDue),
            rows.Where(r => r.AgingBucket == "90+ Days").Sum(r => r.BalanceDue),
            rows.Sum(r => r.BalanceDue),
            rows
        );
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var accounts = await _db.Set<ChartOfAccount>()
            .Where(a => !a.IsDeleted && a.IsActive)
            .ToListAsync();

        var totalAssets = accounts
            .Where(a => a.AccountType == AccountGroupType.Asset)
            .Sum(a => a.CurrentBalance > 0 ? a.CurrentBalance : 0);

        var totalLiabilities = accounts
            .Where(a => a.AccountType == AccountGroupType.Liability)
            .Sum(a => a.CurrentBalance > 0 ? a.CurrentBalance : 0);

        var totalEquity = accounts
            .Where(a => a.AccountType == AccountGroupType.Equity)
            .Sum(a => a.CurrentBalance > 0 ? a.CurrentBalance : 0);

        var totalIncome = accounts
            .Where(a => a.AccountType == AccountGroupType.Income)
            .Sum(a => a.CurrentBalance > 0 ? a.CurrentBalance : 0);

        var totalExpenses = accounts
            .Where(a => a.AccountType == AccountGroupType.Expense)
            .Sum(a => a.CurrentBalance > 0 ? a.CurrentBalance : 0);

        var currentProfitLoss = totalIncome - totalExpenses;
        var totalJournalEntries = await _db.Set<JournalEntry>()
            .CountAsync(j => !j.IsDeleted);

        var recentTransactions = await _db.Set<JournalEntry>()
            .CountAsync(j => !j.IsDeleted && j.CreatedAt >= DateTime.UtcNow.AddDays(-30));

        return new DashboardSummaryDto(
            totalAssets, totalLiabilities, totalEquity,
            currentProfitLoss, totalJournalEntries, recentTransactions
        );
    }

    /// <summary>
    /// Receipts &amp; Payments Statement — a cash/bank fund-position report. Every journal entry
    /// (Posted or Draft, not Cancelled) that touches a cash/bank account is classified as a
    /// transfer (both legs are cash/bank, or a linked Contra voucher), a receipt (net cash
    /// inflow) or a payment (net cash outflow). Receipts and payments are itemised by their
    /// non-cash counterpart account and grouped by cost center; the bottom section is each
    /// cash/bank account's opening → movement → closing over the period.
    ///
    /// An account counts as cash/bank if it is flagged <c>IsCashOrBank</c> or it is the GL
    /// account behind a bank account (<c>acc_bank_accounts</c>), so bank movement shows up with
    /// no set-up and cash-in-hand accounts are picked up once flagged.
    /// </summary>
    public async Task<ReceiptsPaymentsReportDto> GetReceiptsPaymentsAsync(ReceiptsPaymentsFilterDto filter)
    {
        var dateFrom = (filter.DateFrom ?? DateTime.UtcNow).Date;
        var dateToInclusive = (filter.DateTo ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);

        var bankGlAccountIds = await _db.Set<BankAccount>()
            .Where(b => !b.IsDeleted)
            .Select(b => b.ChartOfAccountId)
            .ToListAsync();

        var cashAccounts = await _db.Set<ChartOfAccount>()
            .Where(a => !a.IsDeleted && (a.IsCashOrBank || bankGlAccountIds.Contains(a.Id)))
            .OrderBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.OpeningBalance })
            .ToListAsync();

        // A List (not a HashSet) for the query itself — EF Core / Npgsql only translates
        // List<T>.Contains, not HashSet<T>.Contains. The HashSet is for the in-memory hot paths below.
        var cashIdList = cashAccounts.Select(a => a.Id).ToList();
        var cashIds = cashIdList.ToHashSet();

        if (cashIdList.Count == 0)
            return new ReceiptsPaymentsReportDto(
                new(), 0, new(), 0, 0, new(), 0, new(), 0, 0, 0, 0);

        // Journal entries that touch a cash/bank account on or before the period end. We then pull
        // every line of those entries (not just the cash ones) so transfers can be identified and
        // counterpart accounts found.
        var entryIds = await _db.Set<JournalEntryLine>()
            .Where(l => !l.IsDeleted
                && cashIdList.Contains(l.AccountId)
                && !l.JournalEntry.IsDeleted
                && l.JournalEntry.Status != JournalEntryStatus.Cancelled
                && l.JournalEntry.EntryDate <= dateToInclusive)
            .Select(l => l.JournalEntryId)
            .Distinct()
            .ToListAsync();

        var allLines = await _db.Set<JournalEntryLine>()
            .Where(l => !l.IsDeleted && entryIds.Contains(l.JournalEntryId))
            .Select(l => new LineView(
                l.JournalEntryId,
                l.AccountId,
                l.Account.AccountCode,
                l.Account.AccountName,
                l.DebitAmount,
                l.CreditAmount,
                l.CostCenterId,
                l.CostCenter != null ? l.CostCenter.Code : null,
                l.CostCenter != null ? l.CostCenter.Name : null,
                l.CostCenter != null ? l.CostCenter.ProjectId : null,
                l.JournalEntry.EntryDate,
                l.JournalEntry.EntryNumber,
                l.JournalEntry.Description))
            .ToListAsync();

        var linesByEntry = allLines.GroupBy(l => l.EntryId).ToDictionary(g => g.Key, g => g.ToList());

        var contraEntryIds = (await _db.Set<Voucher>()
            .Where(v => !v.IsDeleted && v.JournalEntryId != null
                && v.VoucherType == VoucherType.Contra
                && entryIds.Contains(v.JournalEntryId.Value))
            .Select(v => v.JournalEntryId!.Value)
            .ToListAsync()).ToHashSet();

        bool IsTransfer(Guid entryId) =>
            contraEntryIds.Contains(entryId) || linesByEntry[entryId].All(l => cashIds.Contains(l.AccountId));

        // ── Bottom portion: per cash/bank account opening → movement → closing.
        // Balances are never scoped by the cost-center / project filter — they are the whole
        // organisation's cash position.
        var balances = new List<CashBankBalanceRowDto>();
        decimal totalOpening = 0, totalPeriodReceipts = 0, totalPeriodPayments = 0, totalClosing = 0;

        foreach (var acc in cashAccounts)
        {
            var accLines = allLines.Where(l => l.AccountId == acc.Id).ToList();
            var opening = acc.OpeningBalance
                + accLines.Where(l => l.EntryDate < dateFrom).Sum(l => l.Debit - l.Credit);
            var receipts = accLines.Where(l => l.EntryDate >= dateFrom && l.EntryDate <= dateToInclusive).Sum(l => l.Debit);
            var payments = accLines.Where(l => l.EntryDate >= dateFrom && l.EntryDate <= dateToInclusive).Sum(l => l.Credit);
            var closing = opening + receipts - payments;

            balances.Add(new CashBankBalanceRowDto(acc.Id, acc.AccountCode, acc.AccountName, opening, receipts, payments, closing));
            totalOpening += opening;
            totalPeriodReceipts += receipts;
            totalPeriodPayments += payments;
            totalClosing += closing;
        }

        // ── Left / right portions: classify each in-period entry.
        // Cost-center dictionary keys use Guid.Empty for "no cost center" (unallocated / head office).
        var transfers = new List<TransferRowDto>();
        var receiptAcc = new Dictionary<(Guid Cc, Guid Acct), decimal>();
        var paymentAcc = new Dictionary<(Guid Cc, Guid Acct), decimal>();
        var ccMeta = new Dictionary<Guid, (string Code, string Name, Guid? ProjectId)>();
        var acctMeta = new Dictionary<Guid, (string Code, string Name)>();

        var periodEntryIds = allLines
            .Where(l => l.EntryDate >= dateFrom && l.EntryDate <= dateToInclusive)
            .Select(l => l.EntryId)
            .Distinct();

        bool PassesScope(LineView nc) =>
            (!filter.CostCenterId.HasValue || nc.CostCenterId == filter.CostCenterId.Value)
            && (!filter.ProjectId.HasValue || nc.CostCenterProjectId == filter.ProjectId.Value);

        void Remember(LineView nc)
        {
            ccMeta[nc.CostCenterId ?? Guid.Empty] = (nc.CostCenterCode ?? "", nc.CostCenterName ?? "Unallocated / Head Office", nc.CostCenterProjectId);
            acctMeta[nc.AccountId] = (nc.AccountCode, nc.AccountName);
        }

        foreach (var entryId in periodEntryIds)
        {
            var lines = linesByEntry[entryId];
            var cashLines = lines.Where(l => cashIds.Contains(l.AccountId)).ToList();
            var nonCashLines = lines.Where(l => !cashIds.Contains(l.AccountId)).ToList();
            var cashDebit = cashLines.Sum(l => l.Debit);
            var cashCredit = cashLines.Sum(l => l.Credit);

            if (IsTransfer(entryId))
            {
                var first = cashLines.First();
                transfers.Add(new TransferRowDto(
                    first.EntryDate,
                    first.EntryNumber,
                    cashLines.Where(l => l.Credit > 0).Select(l => l.AccountName).FirstOrDefault() ?? "",
                    cashLines.Where(l => l.Debit > 0).Select(l => l.AccountName).FirstOrDefault() ?? "",
                    first.Narration ?? "",
                    Math.Max(cashDebit, cashCredit)));
                continue;
            }

            var net = cashDebit - cashCredit;
            if (net > 0)
            {
                foreach (var nc in nonCashLines.Where(l => l.Credit > 0 && PassesScope(l)))
                {
                    var key = (nc.CostCenterId ?? Guid.Empty, nc.AccountId);
                    receiptAcc[key] = receiptAcc.GetValueOrDefault(key) + nc.Credit;
                    Remember(nc);
                }
            }
            else if (net < 0)
            {
                foreach (var nc in nonCashLines.Where(l => l.Debit > 0 && PassesScope(l)))
                {
                    var key = (nc.CostCenterId ?? Guid.Empty, nc.AccountId);
                    paymentAcc[key] = paymentAcc.GetValueOrDefault(key) + nc.Debit;
                    Remember(nc);
                }
            }
        }

        List<ReceiptsPaymentsGroupDto> BuildGroups(Dictionary<(Guid Cc, Guid Acct), decimal> map) => map
            .GroupBy(kv => kv.Key.Cc)
            .Select(g =>
            {
                var meta = ccMeta.GetValueOrDefault(g.Key, ("", "Unallocated / Head Office", null));
                var rows = g
                    .Select(kv => new ReceiptsPaymentsRowDto(kv.Key.Acct, acctMeta[kv.Key.Acct].Code, acctMeta[kv.Key.Acct].Name, kv.Value))
                    .OrderBy(r => r.AccountCode)
                    .ToList();
                return new ReceiptsPaymentsGroupDto(
                    g.Key == Guid.Empty ? null : g.Key,
                    meta.Code, meta.Name, meta.ProjectId, rows.Sum(r => r.Amount), rows);
            })
            .OrderBy(g => g.CostCenterName)
            .ToList();

        var receiptGroups = BuildGroups(receiptAcc);
        var paymentGroups = BuildGroups(paymentAcc);
        var totalReceiptsExcl = receiptGroups.Sum(g => g.Subtotal);
        var totalTransfers = transfers.Sum(t => t.Amount);
        var totalPayments = paymentGroups.Sum(g => g.Subtotal);

        return new ReceiptsPaymentsReportDto(
            transfers.OrderBy(t => t.EntryDate).ThenBy(t => t.EntryNumber).ToList(),
            totalTransfers,
            receiptGroups,
            totalReceiptsExcl,
            totalReceiptsExcl + totalTransfers,
            paymentGroups,
            totalPayments,
            balances,
            totalOpening,
            totalPeriodReceipts,
            totalPeriodPayments,
            totalClosing);
    }

    /// <summary>Flattened journal-entry-line projection used only by the Receipts &amp; Payments report.</summary>
    private sealed record LineView(
        Guid EntryId,
        Guid AccountId,
        string AccountCode,
        string AccountName,
        decimal Debit,
        decimal Credit,
        Guid? CostCenterId,
        string? CostCenterCode,
        string? CostCenterName,
        Guid? CostCenterProjectId,
        DateTime EntryDate,
        string EntryNumber,
        string? Narration);
}
