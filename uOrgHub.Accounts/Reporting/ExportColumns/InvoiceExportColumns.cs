using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class InvoiceExportColumns
{
    public static List<ExportColumn<InvoiceResponseDto>> Get() =>
    [
        new("invoiceNumber", "Invoice Number", x => x.InvoiceNumber),
        new("customerName", "Customer", x => x.CustomerName),
        new("invoiceDate", "Invoice Date", x => x.InvoiceDate),
        new("dueDate", "Due Date", x => x.DueDate),
        new("status", "Status", x => x.Status.ToString()),
        new("subTotal", "Sub Total", x => x.SubTotal),
        new("taxAmount", "Tax Amount", x => x.TaxAmount),
        new("discountAmount", "Discount", x => x.DiscountAmount),
        new("totalAmount", "Total", x => x.TotalAmount),
        new("paidAmount", "Paid", x => x.PaidAmount),
        new("balanceDue", "Balance Due", x => x.BalanceDue),
        new("notes", "Notes", x => x.Notes),
    ];
}
