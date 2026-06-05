using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Procurement.Reporting.ExportColumns;

public static class PRExportColumns
{
    public static List<ExportColumn<PRResponseDto>> Get() =>
    [
        new("prNumber", "PR Number", x => x.PRNumber),
        new("prDate", "PR Date", x => x.PRDate),
        new("requiredDate", "Required Date", x => x.RequiredDate),
        new("department", "Department", x => x.DepartmentName),
        new("requestedBy", "Requested By", x => x.RequestedByName),
        new("purpose", "Purpose", x => x.Purpose),
        new("status", "Status", x => x.StatusName),
        new("approvedBy", "Approved By", x => x.ApprovedByName),
        new("approvedAt", "Approved At", x => x.ApprovedAt),
        new("rejectionReason", "Rejection Reason", x => x.RejectionReason),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
