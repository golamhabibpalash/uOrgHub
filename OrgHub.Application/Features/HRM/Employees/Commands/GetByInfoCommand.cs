using MediatR;
using OrgHub.Application.Features.HRM.Employees.DTOs;

namespace OrgHub.Application.Features.HRM.Employees.Commands;

public class GetByInfoCommand : IRequest<List<EmployeeDto>>
{
    public string Info { get; set; }
    public GetByInfoCommand(string info)
    {
        Info = info;
    }
}

