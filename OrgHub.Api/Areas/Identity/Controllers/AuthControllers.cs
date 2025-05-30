using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Application.Features.Others.Interfaces;

namespace OrgHub.Api.Areas.Identity.Controllers;

/// <summary>
/// This Controller is used for authentication related operations.
/// </summary>
[ApiController]
[Route("api/[area]/[controller]")]
[Area("Identity")]
public class AuthController(IAuthService authService, ILoggingService loggingService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILoggingService _loggingService = loggingService;

    /// <summary>
    /// Authorization controller constructor.
    /// </summary>

    /// <summary>
    /// Token based authentication for login.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _authService.LoginAsync(email, password);
        
        return Ok(result);
    }


    /// <summary>
    /// for refresh token authentication.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(result);
    }

    /// <summary>
    /// To Register as a new user.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var result = await _authService.RegisterUserAsync(dto);
        if (!result)
            return BadRequest("Registration failed");

        return Ok("User registered successfully");
    }

    /// <summary>
    /// This method is used to update existing user information.
    /// </summary>
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        var result = await _authService.UpdateUserAsync(dto);
        if (!result)
            return BadRequest("User update failed");
        return Ok("User updated successfully");
    }
}
