using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.HRM.Employees.Handlers;

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeService _service;
    public UpdateEmployeeHandler(IEmployeeService service)
    {
        _service = service;
    }
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _service.GetByIdAsync(request.Employee.Id);

        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.Employee.Id} not found.");
        }

        // Update the employee properties
        employee = request.Employee;

        // Save the updated employee to the Service
        await _service.UpdateAsync(employee);

        var updatedEmployee = await _service.GetByIdAsync(request.Employee.Id);

        if (updatedEmployee == null)
        {
            throw new InvalidOperationException($"Failed to retrieve updated employee with ID {request.Employee.Id}.");
        }

        return updatedEmployee;
    }
}