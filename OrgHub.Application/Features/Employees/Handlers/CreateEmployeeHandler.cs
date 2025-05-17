using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.Models;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Employees.Handlers;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    public CreateEmployeeHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            EmployeeCode = Guid.NewGuid().ToString(), // Generate unique EmployeeId
            Name = request.Name,
            Designation = request.Designation,
            Phone = request.Phone,
            Email = request.Email,
            JoiningDate = DateTime.UtcNow, // Set default joining date
            IsActive = true, // Set default active status
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 2
        };

        // Save the employee to the repository
        var savedEmployee = await _employeeRepository.AddAsync(employee);

        // Map the saved Employee entity to EmployeeDto
        var employeeDto = new EmployeeDto
        {
            Id = savedEmployee.Id,
            EmployeeCode = savedEmployee.EmployeeCode,
            Name = savedEmployee.Name,
            Designation = savedEmployee.Designation,
            Phone = savedEmployee.Phone,
            Email = savedEmployee.Email
        };

        return employeeDto;
    }
}