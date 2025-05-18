using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Application.Features.Employees.Interfaces;

namespace OrgHub.Application.Features.Employees.Handlers;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeService _service;
    public CreateEmployeeHandler(IEmployeeService service)
    {
        _service = service;
    }
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var savedEmployee = await _service.AddAsync(request.Employee);
        return savedEmployee;
    }
}