using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;

namespace OrgHub.Application.Features.HRM.Employees.Handlers;

public class GetByInfoHandler : IRequestHandler<GetByInfoCommand, List<EmployeeDto>>
{
    private readonly IEmployeeService _employeeService;
    public GetByInfoHandler(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    public async Task<List<EmployeeDto>> Handle(GetByInfoCommand request, CancellationToken cancellationToken)
    {
        var list = await _employeeService.GetByInfoAsync(request.Info);
        return list;
    }
}
