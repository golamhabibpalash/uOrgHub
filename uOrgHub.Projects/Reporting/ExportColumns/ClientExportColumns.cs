using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ClientExportColumns
{
    public static List<ExportColumn<ClientResponseDto>> Get() =>
    [
        new("clientCode", "Client Code", x => x.ClientCode),
        new("companyName", "Company Name", x => x.CompanyName),
        new("contactPerson", "Contact Person", x => x.ContactPerson),
        new("email", "Email", x => x.Email),
        new("phone", "Phone", x => x.Phone),
        new("address", "Address", x => x.Address),
        new("clientType", "Client Type", x => x.ClientType.ToString()),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
