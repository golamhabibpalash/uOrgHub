using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.Employees.Handlers;

public class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    public DeleteEmployeeHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }
    public async Task<EmployeeDto> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id);
        if (employee == null)
        {
            return null; // or throw an exception if you prefer
        }
        // Delete the employee from the repository
        await _employeeRepository.DeleteAsync(request.Id);

        // Map the deleted Employee entity to EmployeeDto
        var employeeDto = _mapper.Map<EmployeeDto>(employee);
        return employeeDto;
    }
}