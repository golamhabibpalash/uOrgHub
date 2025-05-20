using MediatR;
using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Commands;

public class AssignPermissionToRoleCommand : IRequest<RolePermissionsDto>
{
    public required RolePermissionsDto RolePermissionsDto { get; set; }
}
