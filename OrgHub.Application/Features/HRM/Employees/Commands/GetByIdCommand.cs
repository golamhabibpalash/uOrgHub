using MediatR;
using OrgHub.Application.Features.HRM.Employees.DTOs;

namespace OrgHub.Application.Features.HRM.Employees.Commands;

public class GetByIdCommand : IRequest<EmployeeDto>
{
    public int Id { get; set; }
    public GetByIdCommand(int id)
    {
        Id = id;
    }
}
