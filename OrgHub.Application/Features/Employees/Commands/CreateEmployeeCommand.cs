using MediatR;
using OrgHub.Application.Features.Employees.DTOs;

namespace OrgHub.Application.Features.Employees.Commands;

public class CreateEmployeeCommand : IRequest<EmployeeDto>
{
    public required EmployeeDto Employee { get; set; }

}