using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.DTOs;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("stats")]
    [RequireClaim(Claims.Self.ViewProfile)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }
}
