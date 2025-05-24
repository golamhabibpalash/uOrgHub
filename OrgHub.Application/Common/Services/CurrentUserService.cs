using Microsoft.AspNetCore.Http;
using OrgHub.Application.Common.Interfaces;
using System.Security.Claims;

namespace OrgHub.Application.Common.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid? UserId =>
        IsAuthenticated ? Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString()) : null;

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<Claim> Claims =>
        _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
}
