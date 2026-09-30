using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth/menu")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IPermissionService _permissions;

    public MenuController(IMenuService menuService, IPermissionService permissions)
    {
        _menuService = menuService;
        _permissions = permissions;
    }

    [HttpGet]
    [RequireClaim(Claims.Self.ViewProfile)]
    public async Task<IActionResult> GetAuthorizedMenu()
    {
        // Permissions come from the same source PermissionMiddleware enforces with, not the token —
        // the token no longer carries them (see IJwtService.GenerateAccessToken).
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userClaims = await _permissions.GetUserClaimsAsync(userId);
        var userRoles = await _permissions.GetUserRolesAsync(userId);

        var menu = _menuService.GetAuthorizedMenu(userClaims, userRoles);
        return Ok(ApiResponse<List<MenuItemDto>>.Ok(menu));
    }
}
