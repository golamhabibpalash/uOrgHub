using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.Employees.Handlers;

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    public UpdateEmployeeHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id);

        if (employee == null)
        {
            return null; // or throw an exception if you prefer
        }

        // Update the employee properties
        employee.Name = request.Name;
        employee.Designation = request.Designation;
        employee.Phone = request.Phone;
        employee.Email = request.Email;
        employee.JoiningDate = request.JoiningDate;
        employee.IsActive = request.IsActive;

        // Save the updated employee to the repository
        await _employeeRepository.UpdateAsync(employee);

        var updatedEmployee = await _employeeRepository.GetByIdAsync(request.Id);

        if (updatedEmployee==null)
        {
            return null;
        }
        // Map the updated Employee entity to EmployeeDto
        var employeeDto = _mapper.Map<EmployeeDto>(updatedEmployee);
        return employeeDto;
    }
}