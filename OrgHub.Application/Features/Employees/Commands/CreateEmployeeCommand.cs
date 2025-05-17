using MediatR;
using OrgHub.Application.Features.Employees.Models;

namespace OrgHub.Application.Features.Employees.Commands;

public class CreateEmployeeCommand : IRequest<EmployeeDto>
{
    public required string Name { get; set; }
    public required string Designation { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }
}