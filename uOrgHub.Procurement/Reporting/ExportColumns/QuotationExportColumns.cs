using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Procurement.Reporting.ExportColumns;

public static class QuotationExportColumns
{
    public static List<ExportColumn<VendorQuotationResponseDto>> Get() =>
    [
        new("quotationNumber", "Quotation Number", x => x.QuotationNumber),
        new("rfqNumber", "RFQ Number", x => x.RFQNumber),
        new("vendorName", "Vendor Name", x => x.VendorName),
        new("quotationDate", "Quotation Date", x => x.QuotationDate),
        new("validUntil", "Valid Until", x => x.ValidUntil),
        new("status", "Status", x => x.StatusName),
        new("totalAmount", "Total Amount", x => x.TotalAmount),
        new("deliveryDays", "Delivery Days", x => x.DeliveryDays),
        new("paymentTerms", "Payment Terms", x => x.PaymentTerms),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
