using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Procurement.Reporting.ExportColumns;

public static class VendorExportColumns
{
    public static List<ExportColumn<VendorResponseDto>> Get() =>
    [
        new("vendorCode", "Vendor Code", x => x.VendorCode),
        new("companyName", "Company Name", x => x.CompanyName),
        new("contactPerson", "Contact Person", x => x.ContactPerson),
        new("email", "Email", x => x.Email),
        new("phone", "Phone", x => x.Phone),
        new("address", "Address", x => x.Address),
        new("vendorType", "Vendor Type", x => x.VendorTypeName),
        new("status", "Status", x => x.StatusName),
        new("tin", "TIN", x => x.TIN),
        new("bin", "BIN", x => x.BIN),
        new("tradeLicense", "Trade License", x => x.TradeLicense),
        new("creditLimit", "Credit Limit", x => x.CreditLimit),
        new("paymentTermDays", "Payment Terms (Days)", x => x.PaymentTermDays),
    ];
}
