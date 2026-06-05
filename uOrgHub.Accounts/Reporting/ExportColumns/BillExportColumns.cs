using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class BillExportColumns
{
    public static List<ExportColumn<BillResponseDto>> Get() =>
    [
        new("billNumber", "Bill Number", x => x.BillNumber),
        new("vendorBillNumber", "Vendor Bill #", x => x.VendorBillNumber),
        new("vendorName", "Vendor", x => x.VendorName),
        new("billDate", "Bill Date", x => x.BillDate),
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
