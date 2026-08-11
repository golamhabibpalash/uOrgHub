using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class VoucherExportColumns
{
    public static List<ExportColumn<VoucherResponseDto>> Get() =>
    [
        new("voucherNumber", "Voucher Number", x => x.VoucherNumber),
        new("voucherType", "Voucher Type", x => x.VoucherType.ToString()),
        new("voucherDate", "Voucher Date", x => x.VoucherDate),
        new("name", "Name", x => x.Name),
        new("section", "Section", x => x.Section),
        new("description", "Description", x => x.Description),
        new("debitAccountName", "Debit Account", x => x.DebitAccountName),
        new("creditAccountName", "Credit Account", x => x.CreditAccountName),
        new("amount", "Amount", x => x.Amount),
        new("status", "Status", x => x.Status.ToString()),
        new("journalEntryNumber", "Journal Entry", x => x.JournalEntryNumber),
        new("createdBy", "Created By", x => x.CreatedBy),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}