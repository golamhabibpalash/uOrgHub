using MediatR;
using OrgHub.Application.Features.HRM.Employees.DTOs;

namespace OrgHub.Application.Features.HRM.Employees.Commands;

/// <summary>
/// Command to update an employee.
/// </summary>
public class UpdateEmployeeCommand : IRequest<EmployeeDto>
{
    public required EmployeeDto Employee { get; set; }
}