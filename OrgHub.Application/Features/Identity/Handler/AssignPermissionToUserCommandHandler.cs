using MediatR;
using OrgHub.Application.Features.Identity.Commands;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;

namespace OrgHub.Application.Features.Identity.Handler;

public class AssignPermissionToUserCommandHandler : IRequestHandler<AssignPermissionToUserCommand, UserPermissionsDto>
{

    private readonly IUserPermissionService _service;

    public AssignPermissionToUserCommandHandler(IUserPermissionService service)
    {
        _service = service;
    }

    public async Task<UserPermissionsDto> Handle(AssignPermissionToUserCommand request, CancellationToken cancellationToken)
    {
        return await _service.AssignPermissionsAsync(request.UserPermissionsDto);
    }
}