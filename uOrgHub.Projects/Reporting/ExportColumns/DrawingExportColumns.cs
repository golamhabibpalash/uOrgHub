using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class DrawingExportColumns
{
    public static List<ExportColumn<DrawingResponseDto>> Get() =>
    [
        new("drawingNumber", "Drawing Number", x => x.DrawingNumber),
        new("title", "Title", x => x.Title),
        new("revision", "Revision", x => x.Revision),
        new("discipline", "Discipline", x => x.Discipline.ToString()),
        new("status", "Status", x => x.Status.ToString()),
        new("issuedDate", "Issued Date", x => x.IssuedDate),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
