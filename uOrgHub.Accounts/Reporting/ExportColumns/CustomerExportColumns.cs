using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class CustomerExportColumns
{
    public static List<ExportColumn<CustomerResponseDto>> Get() =>
    [
        new("customerCode", "Customer Code", x => x.CustomerCode),
        new("name", "Name", x => x.Name),
        new("contactPerson", "Contact Person", x => x.ContactPerson),
        new("email", "Email", x => x.Email),
        new("phone", "Phone", x => x.Phone),
        new("address", "Address", x => x.Address),
        new("tin", "TIN", x => x.TIN),
        new("bin", "BIN", x => x.BIN),
        new("creditLimit", "Credit Limit", x => x.CreditLimit),
        new("paymentTermsDays", "Payment Terms (Days)", x => x.PaymentTermsDays),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
    ];
}
