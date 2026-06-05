using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class SafetyIncidentExportColumns
{
    public static List<ExportColumn<SafetyIncidentResponseDto>> Get() =>
    [
        new("incidentNumber", "Incident Number", x => x.IncidentNumber),
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("incidentDate", "Incident Date", x => x.IncidentDate),
        new("location", "Location", x => x.Location),
        new("severity", "Severity", x => x.Severity.ToString()),
        new("status", "Status", x => x.Status.ToString()),
        new("injuredPersonName", "Injured Person", x => x.InjuredPersonName),
        new("injuryType", "Injury Type", x => x.InjuryType),
        new("rootCause", "Root Cause", x => x.RootCause),
        new("correctiveAction", "Corrective Action", x => x.CorrectiveAction),
        new("preventiveAction", "Preventive Action", x => x.PreventiveAction),
        new("investigationDate", "Investigation Date", x => x.InvestigationDate),
        new("closedDate", "Closed Date", x => x.ClosedDate),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
