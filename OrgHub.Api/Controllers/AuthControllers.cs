using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.Auth.DTOs;
using OrgHub.Application.Features.Auth.Interfaces;

namespace OrgHub.Api.Controllers;

/// <summary>
/// This Controller is used for authentication related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController:ControllerBase
{
	private readonly IAuthService _authService;

    /// <summary>
    /// Authorization controller constructor.
    /// </summary>
    public AuthController(IAuthService authService)
	{
		_authService = authService;
    }

    /// <summary>
    /// Token based authentication for login.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    [HttpPost("login")]
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
