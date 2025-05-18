using MediatR;
using OrgHub.Application.Features.HRM.Employees.DTOs;

namespace OrgHub.Application.Features.HRM.Employees.Commands;

public class GetAllEmployeesCommand : IRequest<List<EmployeeDto>> { }
