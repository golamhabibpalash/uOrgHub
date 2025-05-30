using MediatR;
using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Commands;

public class LoginCommand : IRequest<AuthResponseDto>
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}