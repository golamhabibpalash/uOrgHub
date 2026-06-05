using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class JobPostingExportColumns
{
    public static List<ExportColumn<JobPostingResponseDto>> Get() =>
    [
        new("jobCode", "Job Code", x => x.JobCode),
        new("title", "Title", x => x.Title),
        new("departmentName", "Department", x => x.DepartmentName),
        new("designationName", "Designation", x => x.DesignationName),
        new("requiredCount", "Required Count", x => x.RequiredCount),
        new("experienceYearsMin", "Exp Min (yrs)", x => x.ExperienceYearsMin),
        new("experienceYearsMax", "Exp Max (yrs)", x => x.ExperienceYearsMax),
        new("salaryMin", "Salary Min", x => x.SalaryMin),
        new("salaryMax", "Salary Max", x => x.SalaryMax),
        new("status", "Status", x => x.Status.ToString()),
        new("postedDate", "Posted Date", x => x.PostedDate),
        new("closingDate", "Closing Date", x => x.ClosingDate),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
