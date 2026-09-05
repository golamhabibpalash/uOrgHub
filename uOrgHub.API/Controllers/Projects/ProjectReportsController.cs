using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Projects.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

/// <summary>
/// Project-scoped accounting reports. Served from the Projects module because they read both the
/// project and the general ledger, and only Projects may reference Accounts — but they are
/// presented under Accounts › Reports, so they are gated on the accounting report claim.
/// </summary>
[Authorize]
[Route("api/v1/projects/reports")]
public class ProjectReportsController : BaseController
{
    private readonly IProjectStatementService _statementService;

    public ProjectReportsController(IProjectStatementService statementService)
    {
        _statementService = statementService;
    }

    [HttpGet("statement/{projectId:guid}")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetProjectStatement(
        Guid projectId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var result = await _statementService.GetStatementAsync(projectId, dateFrom, dateTo);
        return Ok(ApiResponse<ProjectStatementDto>.Ok(result));
    }

    /// <summary>The statement rolled up across every project — organisation-wide totals over a
    /// per-project breakdown, for the "All Projects" option on the report screen.</summary>
    [HttpGet("statement")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetConsolidatedProjectStatement(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var result = await _statementService.GetConsolidatedStatementAsync(dateFrom, dateTo);
        return Ok(ApiResponse<ConsolidatedProjectStatementDto>.Ok(result));
    }
}
