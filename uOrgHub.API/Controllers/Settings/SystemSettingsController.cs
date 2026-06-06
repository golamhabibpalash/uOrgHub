using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;
using uOrgHub.Settings.DTOs;
using uOrgHub.Settings.Services;

namespace uOrgHub.API.Controllers.Settings;

[Authorize]
[Route("api/v1/settings")]
public class SystemSettingsController : BaseController
{
    private readonly ISystemSettingService _service;

    public SystemSettingsController(ISystemSettingService service) => _service = service;

    [HttpGet]
    [RequireClaim(Claims.Settings.SystemSettings.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<SystemSettingResponseDto>>.Ok(result));
    }

    [HttpGet("by-category/{category}")]
    [RequireClaim(Claims.Settings.SystemSettings.View)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var result = await _service.GetByCategoryAsync(category);
        return Ok(ApiResponse<List<SystemSettingResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Settings.SystemSettings.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<SystemSettingResponseDto>.Ok(result));
    }

    [HttpGet("by-key/{key}")]
    [RequireClaim(Claims.Settings.SystemSettings.View)]
    public async Task<IActionResult> GetByKey(string key)
    {
        var result = await _service.GetByKeyAsync(key);
        return Ok(ApiResponse<SystemSettingResponseDto>.Ok(result));
    }

    [HttpGet("active")]
    [RequireClaim(Claims.Settings.SystemSettings.View)]
    public async Task<IActionResult> GetAllActive()
    {
        var result = await _service.GetAllActiveAsync();
        return Ok(ApiResponse<List<SystemSettingResponseDto>>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Settings.SystemSettings.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSystemSettingDto dto)
    {
        var result = await _service.CreateAsync(dto, GetUserName());
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SystemSettingResponseDto>.Ok(result, "Setting created."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Settings.SystemSettings.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSystemSettingDto dto)
    {
        var result = await _service.UpdateAsync(id, dto, GetUserName());
        return Ok(ApiResponse<SystemSettingResponseDto>.Ok(result, "Setting updated."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Settings.SystemSettings.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, GetUserName());
        return Ok(ApiResponse<string>.Ok("Setting deleted."));
    }
}
