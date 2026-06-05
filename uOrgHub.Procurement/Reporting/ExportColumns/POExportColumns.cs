using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Procurement.Reporting.ExportColumns;

public static class POExportColumns
{
    public static List<ExportColumn<POResponseDto>> Get() =>
    [
        new("poNumber", "PO Number", x => x.PONumber),
        new("poDate", "PO Date", x => x.PODate),
        new("expectedDeliveryDate", "Expected Delivery", x => x.ExpectedDeliveryDate),
        new("vendorName", "Vendor Name", x => x.VendorName),
        new("quotationNumber", "Quotation Number", x => x.QuotationNumber),
        new("prNumber", "PR Number", x => x.PRNumber),
        new("status", "Status", x => x.StatusName),
        new("subTotal", "Sub Total", x => x.SubTotal),
        new("taxAmount", "Tax Amount", x => x.TaxAmount),
        new("discountAmount", "Discount Amount", x => x.DiscountAmount),
        new("totalAmount", "Total Amount", x => x.TotalAmount),
        new("paymentTerms", "Payment Terms", x => x.PaymentTerms),
        new("deliveryAddress", "Delivery Address", x => x.DeliveryAddress),
        new("approvedBy", "Approved By", x => x.ApprovedByName),
        new("approvedAt", "Approved At", x => x.ApprovedAt),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
