using MediatR;
using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Commands;

public class AssignPermissionToUserCommand : IRequest<UserPermissionsDto>
{
    public required UserPermissionsDto UserPermissionsDto { get; set; }
}
