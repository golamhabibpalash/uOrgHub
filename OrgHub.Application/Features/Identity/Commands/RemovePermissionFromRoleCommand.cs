using MediatR;
using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Commands;

public class RemovePermissionFromRoleCommand : IRequest<RolePermissionsDto>
{
    public required RolePermissionsDto RolePermissionsDto { get; set; }
}
