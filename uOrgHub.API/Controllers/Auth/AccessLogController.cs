using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Reporting.ExportColumns;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[Authorize]
[Route("api/v1/access-logs")]
public class AccessLogController : BaseController
{
    private readonly IAccessLogService _accessLogService;
    private readonly IExportService _exportService;

    public AccessLogController(IAccessLogService accessLogService, IExportService exportService)
    {
        _accessLogService = accessLogService;
        _exportService = exportService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetLogs([FromQuery] AccessLogFilterRequest request)
    {
        var result = await _accessLogService.GetLogsAsync(request);
        return Ok(ApiResponse<PagedResult<UserAccessLogDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Admin.AccessLogs.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] AccessLogFilterRequest? filter = null)
    {
        var data = await _accessLogService.GetAllLogsExportAsync(filter);
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, AccessLogExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "AccessLogs"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("my")]
    [RequireClaim(Claims.Self.ViewAccessLogs)]
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
