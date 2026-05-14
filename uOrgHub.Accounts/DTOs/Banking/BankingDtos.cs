using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Banking;

public class CreateBankAccountDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? RoutingNumber { get; set; }
    public string Currency { get; set; } = "BDT";
    public decimal OpeningBalance { get; set; } = 0;
    public Guid ChartOfAccountId { get; set; }
}

public class UpdateBankAccountDto
{
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? RoutingNumber { get; set; }
    public bool IsActive { get; set; }
}

public class BankAccountResponseDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? RoutingNumber { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public Guid ChartOfAccountId { get; set; }
}

public class CreateBankTransactionDto
{
    public Guid BankAccountId { get; set; }
    public BankTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? Payee { get; set; }
    public Guid? JournalEntryId { get; set; }
}

public class BankTransactionResponseDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public BankTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? Payee { get; set; }
    public bool IsReconciled { get; set; }
    public Guid? JournalEntryId { get; set; }
}
