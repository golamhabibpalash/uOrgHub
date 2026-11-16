using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs.Reports;

/// <summary>
/// One posted journal entry line attributed to the project, as it appears in the statement's
/// transaction ledger.
///
/// <see cref="PaidTo"/> / <see cref="ReceivedFrom"/> are the counterparty named on the voucher
/// that produced the entry (a Debit voucher's "Paid To", a Credit voucher's "Received From").
/// Entries not raised from a voucher — manual journals, contra transfers — carry neither.
///
/// <see cref="CashFlow"/> marks lines from a Credit (receipt) or Debit (payment) voucher charged
/// to this project; every other line leaves it null.
///
/// <see cref="IsReversed"/> marks both entries of a voucher that was reversed ("Correct this
/// voucher") when the original and its reversal both fall in the period: the pair nets to zero, so
/// the statement hides it by default and shows only the corrected voucher that replaced it.
///
/// <see cref="IsCashBank"/> marks the money side of an entry — the cash/bank account a voucher
/// paid from or received into (both sides of a contra), or any account flagged cash-or-bank. A
/// voucher stamps the project's cost center on both of its lines, so without this the project's
/// ledger would carry every payment twice: once as the cost, once as the cash leaving the bank.
///
/// <see cref="RunningNet"/> is carried over the full ledger when the statement is requested
/// without a transaction query; with one, it is recomputed over the rows that query keeps visible.
/// </summary>
public record ProjectStatementRowDto(
    DateTime EntryDate,
    string EntryNumber,
    string? ReferenceNumber,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountGroupType AccountType,
    string? Narration,
    string? PaidTo,
    string? ReceivedFrom,
    ProjectCashFlowType? CashFlow,
    string CostCenterName,
    decimal Debit,
    decimal Credit,
    decimal RunningNet,
    bool IsReversed = false,
    bool IsCashBank = false
);

/// <summary>
/// The filters and sort the report screen applies to the statement's transaction list, so a
/// printed or downloaded statement can carry exactly the rows the user is looking at. Every
/// field is optional; left empty, the list is the full ledger in posting order.
///
/// These narrow the transaction list only. Summary figures and the account breakdown never depend
/// on them. <see cref="IncludeReversed"/> and <see cref="IncludeCashBank"/> default to hiding those
/// lines, the same default as the report screen.
/// </summary>
public class ProjectStatementTransactionQuery
{
    public ProjectStatementTransactionFilter TransactionType { get; set; } = ProjectStatementTransactionFilter.All;

    /// <summary>Case-insensitive text match across entry #, reference, account, narration, parties and cost center.</summary>
    public string? Search { get; set; }

    public Guid? AccountId { get; set; }

    public ProjectStatementLineSide? Side { get; set; }

    /// <summary>Party name, matched case-insensitively against <see cref="ProjectStatementRowDto.PaidTo"/>.</summary>
    public string? PaidTo { get; set; }

    /// <summary>Party name, matched case-insensitively against <see cref="ProjectStatementRowDto.ReceivedFrom"/>.</summary>
    public string? ReceivedFrom { get; set; }

    /// <summary>A <see cref="ProjectStatementRowDto"/> property name; empty keeps posting order.</summary>
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }

    /// <summary>Show reversed voucher pairs (see <see cref="ProjectStatementRowDto.IsReversed"/>). Hidden by default.</summary>
    public bool IncludeReversed { get; set; }

    /// <summary>Show the cash/bank side of entries (see <see cref="ProjectStatementRowDto.IsCashBank"/>). Hidden by default.</summary>
    public bool IncludeCashBank { get; set; }
}

/// <summary>
/// Period totals for one account the project was charged against. Built without reversed pairs
/// and cash/bank lines (see <see cref="ProjectStatementRowDto"/>) — neither changes an account's
/// net, but both inflate its debit and credit columns with money that is not project cost.
/// </summary>
public record ProjectStatementAccountDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountGroupType AccountType,
    decimal Debit,
    decimal Credit,
    decimal Net
);

/// <summary>
/// A project's accounting activity over a date range: what it has cost, what it has earned, and
/// the cash that moved through it, with the ledger lines those figures come from.
///
/// Every figure is derived from *posted* journal entry lines carrying one of the project's cost
/// centers — the same basis <see cref="Services.ProjectFinancialService"/> uses, so the statement
/// and the project's financial summary can never disagree.
/// </summary>
public record ProjectStatementDto(
    Guid ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal ContractValue,
    DateTime? DateFrom,
    DateTime? DateTo,

    /// <summary>Net cost booked before <see cref="DateFrom"/>, so the period reads as a movement.</summary>
    decimal OpeningSpend,
    decimal PeriodExpense,
    decimal PeriodIncome,
    decimal ClosingSpend,

    /// <summary>Money received on posted Credit Vouchers dated in the period.</summary>
    decimal Receipts,

    /// <summary>Money paid out on posted Debit Vouchers dated in the period.</summary>
    decimal Payments,
    decimal NetCashPosition,

    List<ProjectStatementAccountDto> ByAccount,
    List<ProjectStatementRowDto> Rows
);

/// <summary>
/// One project's headline figures as it appears in the consolidated (all-projects) statement —
/// the same numbers the project's own <see cref="ProjectStatementDto"/> summary band carries,
/// so a row here reconciles to that project's individual statement.
/// </summary>
public record ConsolidatedProjectRowDto(
    Guid ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal ContractValue,
    decimal OpeningSpend,
    decimal PeriodExpense,
    decimal PeriodIncome,
    decimal ClosingSpend,
    decimal Receipts,
    decimal Payments,
    decimal NetCashPosition
);

/// <summary>
/// The Project Statement rolled up across every project for a date range: organisation-wide
/// totals over a per-project breakdown. Built on the same posted-line basis as
/// <see cref="ProjectStatementDto"/> — the transaction ledger is dropped because it would be
/// unbounded across all projects, but every summary figure still ties out to the individual
/// statements.
/// </summary>
public record ConsolidatedProjectStatementDto(
    DateTime? DateFrom,
    DateTime? DateTo,
    decimal ContractValue,
    decimal OpeningSpend,
    decimal PeriodExpense,
    decimal PeriodIncome,
    decimal ClosingSpend,
    decimal Receipts,
    decimal Payments,
    decimal NetCashPosition,
    List<ConsolidatedProjectRowDto> Projects
);
