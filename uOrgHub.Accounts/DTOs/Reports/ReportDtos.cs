using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Models;

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

public record AccountLedgerGroupDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountGroupName,
    AccountGroupType AccountType,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    List<AccountLedgerRowDto> Rows
);

public record DayBookFilterDto(
    DateTime? DateFrom,
    DateTime? DateTo,
    string? Type
);

public record DayBookRowDto(
    Guid Id,
    DateTime EntryDate,
    string EntryNumber,
    string? ReferenceNumber,
    string Description,
    string Status,
    string Type,
    decimal DebitTotal,
    decimal CreditTotal,
    string CreatedBy
);

public record DayBookReportDto(
    PagedResult<DayBookRowDto> Rows,
    decimal TotalDebit,
    decimal TotalCredit
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
    Guid Id,
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

// ── Receipts & Payments Statement ──────────────────────────────────────────
// A cash/bank fund-position report. Driven off journal entry lines that hit an
// account flagged IsCashOrBank, over non-cancelled entries in a date range.

public record ReceiptsPaymentsFilterDto(
    DateTime? DateFrom,
    DateTime? DateTo,
    Guid? FiscalYearId,
    Guid? CostCenterId,
    Guid? ProjectId
);

/// <summary>One counterpart account (income source or expense head) within a cost-center group.</summary>
public record ReceiptsPaymentsRowDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal Amount
);

/// <summary>Receipts or payments rolled up under one cost center (null = unallocated / head office).</summary>
public record ReceiptsPaymentsGroupDto(
    Guid? CostCenterId,
    string CostCenterCode,
    string CostCenterName,
    Guid? ProjectId,
    decimal Subtotal,
    List<ReceiptsPaymentsRowDto> Rows
);

/// <summary>A single transfer between the organisation's own cash/bank accounts.</summary>
public record TransferRowDto(
    DateTime EntryDate,
    string EntryNumber,
    string FromAccount,
    string ToAccount,
    string Narration,
    decimal Amount
);

/// <summary>Per cash/bank account movement over the period.</summary>
public record CashBankBalanceRowDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal Opening,
    decimal Receipts,
    decimal Payments,
    decimal Closing
);

public record ReceiptsPaymentsReportDto(
    // Left portion
    List<TransferRowDto> Transfers,
    decimal TotalTransfers,
    List<ReceiptsPaymentsGroupDto> Receipts,
    decimal TotalReceiptsExclTransfers,
    decimal TotalReceiptsInclTransfers,
    // Right portion
    List<ReceiptsPaymentsGroupDto> Payments,
    decimal TotalPayments,
    // Bottom portion
    List<CashBankBalanceRowDto> Balances,
    decimal TotalOpening,
    decimal TotalPeriodReceipts,
    decimal TotalPeriodPayments,
    decimal TotalClosing
);
