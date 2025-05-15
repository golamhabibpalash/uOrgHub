using MediatR;
using OrgHub.Application.Features.Employees.Models;

namespace OrgHub.Application.Features.Employees.Commands;

public class CreateEmployeeCommand : IRequest<EmployeeDto>
{
    public string FullName { get; set; }
    public string Department { get; set; }
}
