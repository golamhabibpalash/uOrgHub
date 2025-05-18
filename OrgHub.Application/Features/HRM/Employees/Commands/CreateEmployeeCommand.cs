using MediatR;
using OrgHub.Application.Features.HRM.Employees.DTOs;

namespace OrgHub.Application.Features.HRM.Employees.Commands;

public class CreateEmployeeCommand : IRequest<EmployeeDto>
{
    public required EmployeeDto Employee { get; set; }

}