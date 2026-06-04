using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth/menu")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService) => _menuService = menuService;

    [HttpGet]
    [RequireClaim(Claims.Self.ViewProfile)]
    public IActionResult GetAuthorizedMenu()
    {
        var userClaims = User.FindAll("permission").Select(c => c.Value).ToList();
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var menu = _menuService.GetAuthorizedMenu(userClaims, userRoles);
        return Ok(ApiResponse<List<MenuItemDto>>.Ok(menu));
    }
}
