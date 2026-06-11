using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Reports;

public record ReportFilterDto(
    DateTime? DateFrom,
    DateTime? DateTo,
    Guid? FiscalYearId,
    AccountGroupType? AccountType,
    Guid? AccountGroupId,
    Guid? AccountId,
    string? Status,
    string? CreatedBy
);

public record TrialBalanceRowDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountGroupName,
    AccountGroupType AccountType,
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal Debit,
    decimal Credit,
    decimal ClosingDebit,
    decimal ClosingCredit
);

public record TrialBalanceResponseDto(
    List<TrialBalanceRowDto> Rows,
    decimal TotalOpeningDebit,
    decimal TotalOpeningCredit,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal TotalClosingDebit,
    decimal TotalClosingCredit
);

public record GeneralLedgerRowDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountGroupName,
    AccountGroupType AccountType,
    decimal OpeningBalance,
    decimal Debit,
    decimal Credit,
    decimal ClosingBalance
);

public record IncomeStatementLineDto(
    string Label,
    decimal Amount,
    bool IsBold,
    List<IncomeStatementLineDto>? Children
);

public record IncomeStatementDto(
    decimal TotalRevenue,
    decimal CostOfSales,
    decimal GrossProfit,
    decimal TotalExpenses,
    decimal NetProfit,
    List<IncomeStatementLineDto> Lines
);

public record BalanceSheetLineDto(
    string Label,
    decimal Amount,
    bool IsBold,
    List<BalanceSheetLineDto>? Children
);

public record BalanceSheetDto(
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    List<BalanceSheetLineDto> Lines
);

public record AccountLedgerRowDto(
    DateTime EntryDate,
    string EntryNumber,
    string? ReferenceNumber,
    string Narration,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance
);

public record DayBookRowDto(
    DateTime EntryDate,
    string EntryNumber,
    string? ReferenceNumber,
    string Description,
    string Status,
    decimal DebitTotal,
    decimal CreditTotal,
    string CreatedBy
);

public record ChartOfAccountsReportRowDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountGroupName,
    AccountGroupType AccountType,
    decimal CurrentBalance,
    bool IsActive,
    string? CustomCode
);

public record JournalEntryReportRowDto(
    string EntryNumber,
    DateTime EntryDate,
    string? ReferenceNumber,
    string Description,
    decimal TotalDebit,
    decimal TotalCredit,
    string Status,
    string CreatedBy,
    DateTime CreatedAt
);

public record AccountGroupSummaryRowDto(
    Guid GroupId,
    string GroupCode,
    string GroupName,
    AccountGroupType GroupType,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Balance,
    int AccountCount
);

public record DashboardSummaryDto(
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    decimal CurrentProfitLoss,
    int TotalJournalEntries,
    int RecentTransactions
);

public record AgingRowDto(
    Guid Id,
    string CustomerOrVendor,
    string DocumentNumber,
    DateTime DocumentDate,
    DateTime DueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    int DaysOverdue,
    string AgingBucket
);

public record AgingSummaryDto(
    decimal CurrentAmount,
    decimal Days1To30,
    decimal Days31To60,
    decimal Days61To90,
    decimal DaysOver90,
    decimal TotalOutstanding,
    List<AgingRowDto> Rows
);
