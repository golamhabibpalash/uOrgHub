namespace OrgHub.Application.Features.Employees.Models;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Department { get; set; } = default!;
}
