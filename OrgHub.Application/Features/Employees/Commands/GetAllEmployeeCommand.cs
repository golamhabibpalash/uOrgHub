using MediatR;
using OrgHub.Application.Features.Employees.Models;

namespace OrgHub.Application.Features.Employees.Commands;

public class GetAllEmployeesCommand : IRequest<List<EmployeeDto>>
{
}
