using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Application.Features.Employees.Interfaces;

namespace OrgHub.Application.Features.Employees.Handlers;

public class GetByIdHandler : IRequestHandler<GetByIdCommand, EmployeeDto>
{
    private readonly IEmployeeService _service;
    public GetByIdHandler(IEmployeeService service)
    {
        _service = service;
    }
    public async Task<EmployeeDto> Handle(GetByIdCommand request, CancellationToken cancellationToken)
    {
        var employee = await _service.GetByIdAsync(request.Id);
        if (employee != null)
        {
            return employee;
        }

        return null;
    }
}
