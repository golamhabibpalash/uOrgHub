using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;

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

    public async Task<List<DayBookRowDto>> GetDayBookAsync(DateTime date)
    {
        var entries = await _db.Set<JournalEntry>()
            .Where(j => !j.IsDeleted && j.EntryDate.Date == date.Date)
            .OrderBy(j => j.EntryNumber)
            .Select(j => new DayBookRowDto(
                j.EntryDate, j.EntryNumber, j.ReferenceNumber,
                j.Description, j.Status.ToString(),
                j.TotalDebit, j.TotalCredit, j.CreatedBy
            ))
            .ToListAsync();

        return entries;
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

    public async Task<List<JournalEntryReportRowDto>> GetJournalEntryReportAsync(ReportFilterDto filter)
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

        var entries = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenBy(j => j.EntryNumber)
            .Select(j => new JournalEntryReportRowDto(
                j.EntryNumber, j.EntryDate, j.ReferenceNumber,
                j.Description, j.TotalDebit, j.TotalCredit,
                j.Status.ToString(), j.CreatedBy, j.CreatedAt
            ))
            .ToListAsync();

        return entries;
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
}
