using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class DPRExportColumns
{
    public static List<ExportColumn<DPRResponseDto>> Get() =>
    [
        new("reportDate", "Report Date", x => x.ReportDate),
        new("weatherCondition", "Weather", x => x.WeatherCondition.ToString()),
        new("workDone", "Work Done", x => x.WorkDone),
        new("issues", "Issues", x => x.Issues),
        new("nextDayPlan", "Next Day Plan", x => x.NextDayPlan),
        new("manpowerCount", "Manpower", x => x.ManpowerCount),
        new("equipmentUsed", "Equipment", x => x.EquipmentUsed),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
