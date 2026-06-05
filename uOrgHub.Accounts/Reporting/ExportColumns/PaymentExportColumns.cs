using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class PaymentExportColumns
{
    public static List<ExportColumn<PaymentResponseDto>> Get() =>
    [
        new("paymentNumber", "Payment Number", x => x.PaymentNumber),
        new("paymentType", "Type", x => x.PaymentType.ToString()),
        new("paymentMethod", "Method", x => x.PaymentMethod.ToString()),
        new("paymentDate", "Payment Date", x => x.PaymentDate),
        new("amount", "Amount", x => x.Amount),
        new("referenceNumber", "Reference", x => x.ReferenceNumber),
        new("chequeNumber", "Cheque #", x => x.ChequeNumber),
        new("customerName", "Customer", x => x.CustomerName),
        new("vendorName", "Vendor", x => x.VendorName),
        new("notes", "Notes", x => x.Notes),
    ];
}
