using MediatR;
using OrgHub.Application.Features.Identity.Commands;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;

namespace OrgHub.Application.Features.Identity.Handler;

public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, RolePermissionsDto>
{
    private readonly IRolePermissionService _rolePermissionService;

    public AssignPermissionToRoleCommandHandler(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }
    public async Task<RolePermissionsDto> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        return await _rolePermissionService.AssignPermissionsAsync(request.RolePermissionsDto);
    }
}