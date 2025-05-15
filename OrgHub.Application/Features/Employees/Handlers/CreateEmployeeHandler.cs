using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.Models;

namespace OrgHub.Application.Features.Employees.Handlers;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new EmployeeDto
        {
            Id = new Random().Next(1, 1000),
            FullName = request.FullName,
            Department = request.Department
        };

        return await Task.FromResult(employee);
    }
}