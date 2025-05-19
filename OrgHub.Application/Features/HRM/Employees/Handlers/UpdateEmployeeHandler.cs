using MediatR;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.HRM.Employees.Handlers;

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeService _service;
    private readonly Func<EmployeeDto, Employee> _mapToEntity;
    private readonly Func<Employee, EmployeeDto> _mapToDto;
    public UpdateEmployeeHandler(IEmployeeService service)
    {
        _service = service;
    }
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _service.GetByIdAsync(request.Employee.Id);

        if (employee == null)
        {
            return null; // or throw an exception if you prefer
        }

        // Update the employee properties
        employee = request.Employee;

        // Save the updated employee to the Service
        await _service.UpdateAsync(employee);

        var updatedEmployee = await _service.GetByIdAsync(request.Employee.Id);

        if (updatedEmployee == null)
        {
            return null;
        }

        return updatedEmployee;
    }
}