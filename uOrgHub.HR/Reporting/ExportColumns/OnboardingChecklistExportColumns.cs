using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class OnboardingChecklistExportColumns
{
    public static List<ExportColumn<OnboardingChecklistResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("description", "Description", x => x.Description),
        new("designationName", "Designation", x => x.DesignationName),
        new("isDefault", "Default", x => x.IsDefault ? "Yes" : "No"),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
