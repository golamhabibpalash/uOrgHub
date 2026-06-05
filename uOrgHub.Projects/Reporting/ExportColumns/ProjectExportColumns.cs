using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ProjectExportColumns
{
    public static List<ExportColumn<ProjectResponseDto>> Get() =>
    [
        new("projectCode", "Project Code", x => x.ProjectCode),
        new("projectName", "Project Name", x => x.ProjectName),
        new("clientName", "Client", x => x.ClientName),
        new("categoryName", "Category", x => x.CategoryName),
        new("location", "Location", x => x.Location),
        new("siteAddress", "Site Address", x => x.SiteAddress),
        new("startDate", "Start Date", x => x.StartDate),
        new("plannedEndDate", "Planned End Date", x => x.PlannedEndDate),
        new("actualEndDate", "Actual End Date", x => x.ActualEndDate),
        new("contractValue", "Contract Value", x => x.ContractValue),
        new("status", "Status", x => x.Status.ToString()),
        new("priority", "Priority", x => x.Priority.ToString()),
        new("description", "Description", x => x.Description),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
