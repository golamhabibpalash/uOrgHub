using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
public class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesCommand, List<EmployeeDto>>
{
    private readonly IEmployeeService _service;

    public GetAllEmployeesHandler(IEmployeeService service)
    {
        _service = service;
    }

    public async Task<List<EmployeeDto>> Handle(GetAllEmployeesCommand request, CancellationToken cancellationToken)
    {
        var list = await _service.GetAllAsync();
        return list;
    }
}
