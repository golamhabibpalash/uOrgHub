using MediatR;
using OrgHub.Application.Features.Employees.DTOs;

namespace OrgHub.Application.Features.Employees.Commands;

public class GetAllEmployeesCommand : IRequest<List<EmployeeDto>> { }
