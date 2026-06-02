using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[Authorize]
public class AccessLogController : BaseController
{
    private readonly IAccessLogService _accessLogService;

    public AccessLogController(IAccessLogService accessLogService)
    {
        _accessLogService = accessLogService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetLogs([FromQuery] AccessLogFilterRequest request)
    {
        var result = await _accessLogService.GetLogsAsync(request);
        return Ok(ApiResponse<PagedResult<UserAccessLogDto>>.Ok(result));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyLogs([FromQuery] AccessLogFilterRequest request)
    {
        var result = await _accessLogService.GetUserLogsAsync(GetUserId(), request);
        return Ok(ApiResponse<PagedResult<UserAccessLogDto>>.Ok(result));
    }

    [HttpGet("summary")]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _accessLogService.GetSummaryAsync();
        return Ok(ApiResponse<AccessLogSummaryDto>.Ok(summary));
    }
}
