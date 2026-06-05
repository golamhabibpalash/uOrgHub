using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Procurement.Reporting.ExportColumns;

public static class GRNExportColumns
{
    public static List<ExportColumn<GRNResponseDto>> Get() =>
    [
        new("grnNumber", "GRN Number", x => x.GRNNumber),
        new("grnDate", "GRN Date", x => x.GRNDate),
        new("poNumber", "PO Number", x => x.PONumber),
        new("warehouseName", "Warehouse", x => x.WarehouseName),
        new("receivedBy", "Received By", x => x.ReceivedByName),
        new("status", "Status", x => x.StatusName),
        new("invoiceNumber", "Invoice Number", x => x.InvoiceNumber),
        new("invoiceDate", "Invoice Date", x => x.InvoiceDate),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
