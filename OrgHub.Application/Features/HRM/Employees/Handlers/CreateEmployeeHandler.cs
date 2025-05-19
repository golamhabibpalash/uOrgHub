using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;

namespace OrgHub.Application.Features.HRM.Employees.Handlers;

public class AuthCommandHandlers : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeService _service;
    public AuthCommandHandlers(IEmployeeService service)
    {
        _service = service;
    }
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var savedEmployee = await _service.AddAsync(request.Employee);
        return savedEmployee;
    }
}