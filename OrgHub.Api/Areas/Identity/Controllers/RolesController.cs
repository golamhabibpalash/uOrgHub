using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.Identity.Interfaces;

namespace OrgHub.Api.Areas.Identity.Controllers;

/// <summary>
/// Controller for managing roles in the Identity area.
/// </summary>
[Route("api/[area]/[controller]")]
[ApiController]
[Area("Identity")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public RolesController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creates a new role or confirms if the role already exists.
    /// </summary>
    /// <param name="roleName">The name of the role to create.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateRole([FromQuery] string roleName)
    {
        var result = await _authService.CreateRoleAsync(roleName);
        if (result)
            return Ok(new { message = "Role created or already exists." });
        return BadRequest(new { message = "Failed to create role." });
    }

    /// <summary>
    /// Adds a user to a specified role.
    /// </summary>
    /// <param name="userId">The ID of the user to add to the role.</param>
    /// <param name="roleName">The name of the role to add the user to.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPost("add-user")]
    public async Task<IActionResult> AddUserToRole([FromQuery] string userId, [FromQuery] string roleName)
    {
        var result = await _authService.AddUserToRoleAsync(userId, roleName);
        if (result)
            return Ok(new { message = "User added to role." });
        return BadRequest(new { message = "Failed to add user to role." });
    }
}
