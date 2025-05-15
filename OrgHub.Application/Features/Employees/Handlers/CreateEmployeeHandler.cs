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
            Id = Guid.NewGuid(),
            EmployeeCode = Guid.NewGuid().ToString(),
            Name = request.Name,
            Designation = request.Designation,
            Phone = request.Phone,
            Email = request.Email
};

        return await Task.FromResult(employee);
    }
}