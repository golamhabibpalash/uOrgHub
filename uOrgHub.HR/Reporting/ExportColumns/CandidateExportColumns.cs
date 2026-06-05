using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class CandidateExportColumns
{
    public static List<ExportColumn<CandidateResponseDto>> Get() =>
    [
        new("firstName", "First Name", x => x.FirstName),
        new("lastName", "Last Name", x => x.LastName),
        new("email", "Email", x => x.Email),
        new("phone", "Phone", x => x.Phone),
        new("totalExperienceYears", "Experience (yrs)", x => x.TotalExperienceYears),
        new("expectedSalary", "Expected Salary", x => x.ExpectedSalary),
        new("currentCompany", "Current Company", x => x.CurrentCompany),
        new("skills", "Skills", x => x.Skills),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
