using MediatR;
using OrgHub.Application.Features.Identity.Commands;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;

namespace OrgHub.Application.Features.Identity.Handler;

public class RemovePermissionFromUserCommandHandler : IRequestHandler<RemovePermissionFromUserCommand, UserPermissionsDto>
{
    private readonly IUserPermissionService _service;

    public RemovePermissionFromUserCommandHandler(IUserPermissionService service)
    {
        _service = service;
    }

    public async Task<UserPermissionsDto> Handle(RemovePermissionFromUserCommand request, CancellationToken cancellationToken)
    {
        return await _service.RemovePermissionsAsync(request.UserPermissionsDto);
    }
}
