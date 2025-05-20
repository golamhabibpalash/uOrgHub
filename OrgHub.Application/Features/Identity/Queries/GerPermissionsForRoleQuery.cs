using MediatR;
using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Queries;

public class GerPermissionsForRoleQuery : IRequest<RolePermissionsDto>
{

}
