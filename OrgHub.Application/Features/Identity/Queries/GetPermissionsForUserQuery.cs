using MediatR;
using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Queries;

public class GetPermissionsForUserQuery : IRequest<UserPermissionsDto>
{
}
