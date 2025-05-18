using MediatR;
using OrgHub.Application.Features.Employees.DTOs;

namespace OrgHub.Application.Features.Employees.Commands;

/// <summary>
/// Command to update an employee.
/// </summary>
public class UpdateEmployeeCommand : IRequest<EmployeeDto>
{
    public required EmployeeDto Employee { get; set; }
}