using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;

namespace OrgHub.Application.Features.HRM.Employees.Handlers;

public class GetByIdHandler : IRequestHandler<GetByIdCommand, EmployeeDto?>
{
    private readonly IEmployeeService _service;
    public GetByIdHandler(IEmployeeService service)
    {
        _service = service;
    }
    public async Task<EmployeeDto?> Handle(GetByIdCommand request, CancellationToken cancellationToken)
    {
        var employee = await _service.GetByIdAsync(request.Id);
        return employee;
    }
}
