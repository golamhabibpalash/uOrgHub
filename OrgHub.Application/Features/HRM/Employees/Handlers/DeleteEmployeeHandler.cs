using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;

namespace OrgHub.Application.Features.HRM.Employees.Handlers;

public class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeService _service;
    public DeleteEmployeeHandler(IEmployeeService service)
    {
        _service = service;
    }

    public async Task<EmployeeDto> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _service.GetByIdAsync(request.Id);
        if (employee == null)
        {
            return null;
        }
        await _service.DeleteAsync(employee.Id);
        return employee;
    }
}
