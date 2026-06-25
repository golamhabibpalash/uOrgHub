using MediatR;
using OrgHub.Application.Features.HRM.Departments.DTOs;

namespace OrgHub.Application.Features.HRM.Departments.Commands;

public class CreateDepartmentCommand : IRequest<CreateDepartmentDtos>
{
    public required CreateDepartmentDtos CreateDepartmentDtos { get; set; }
}
