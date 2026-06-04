using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.DTOs;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Admin;

[ApiController]
[Route("api/v1/theme")]
public class ThemeController : ControllerBase
{
    private readonly IThemeService _theme;

    public ThemeController(IThemeService theme)
    {
        _theme = theme;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var result = await _theme.GetCurrentAsync();
        return Ok(ApiResponse<ThemeSettingsDto>.Ok(result));
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateThemeDto dto)
    {
        var result = await _theme.UpdateAsync(dto);
        return Ok(ApiResponse<ThemeSettingsDto>.Ok(result, "Theme updated"));
    }

    [HttpPost("reset")]
    [Authorize]
    public async Task<IActionResult> Reset()
    {
        var result = await _theme.ResetDefaultAsync();
        return Ok(ApiResponse<ThemeSettingsDto>.Ok(result, "Theme reset to defaults"));
    }
}
