namespace OrgHub.Application.Features.HRM.Employees.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public required string EmployeeCode { get; set; }
    public required string Name { get; set; }
    public required string Designation { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }
}
