using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Features.FixedAssets;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/asset-deployments")]
public class AssetDeploymentsController : BaseController
{
    private readonly IMediator _mediator;
    public AssetDeploymentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Accounts.AssetDeployments.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? fixedAssetId,
        [FromQuery] Guid? projectId, [FromQuery] bool openOnly = false)
    {
        var result = await _mediator.Send(new GetAssetDeploymentsQuery(request, fixedAssetId, projectId, openOnly));
        return Ok(ApiResponse<PagedResult<AssetDeploymentResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.AssetDeployments.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAssetDeploymentByIdQuery(id));
        return Ok(ApiResponse<AssetDeploymentResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.AssetDeployments.Create)]
    public async Task<IActionResult> Deploy([FromBody] DeployAssetDto dto)
    {
        var result = await _mediator.Send(new DeployAssetCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<AssetDeploymentResponseDto>.Ok(result, $"{result.FixedAssetName} deployed to {result.CostCenterName}."));
    }

    [HttpPost("{id:guid}/return")]
    [RequireClaim(Claims.Accounts.AssetDeployments.Edit)]
    public async Task<IActionResult> Return(Guid id, [FromBody] ReturnAssetDto dto)
    {
        var result = await _mediator.Send(new ReturnAssetCommand(id, dto));
        return Ok(ApiResponse<AssetDeploymentResponseDto>.Ok(result, $"{result.FixedAssetName} returned from {result.CostCenterName}."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.AssetDeployments.Delete)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _mediator.Send(new CancelAssetDeploymentCommand(id));
        return Ok(ApiResponse<string>.Ok("Cancelled", "Deployment cancelled."));
    }
}

[Authorize]
[Route("api/v1/accounts/hire-charge-runs")]
public class HireChargeRunsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public HireChargeRunsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.HireCharges.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetHireChargeRunsQuery(request));
        return Ok(ApiResponse<PagedResult<HireChargeRunResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.HireCharges.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllHireChargeRunsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, HireChargeRunExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "EquipmentHireRuns"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    /// <summary>What a run for this date range would charge each project, without posting anything.</summary>
    [HttpGet("preview")]
    [RequireClaim(Claims.Accounts.HireCharges.View)]
    public async Task<IActionResult> Preview([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _mediator.Send(new PreviewHireChargesQuery(fromDate, toDate));
        return Ok(ApiResponse<HireChargePreviewDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.HireCharges.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetHireChargeRunByIdQuery(id));
        return Ok(ApiResponse<HireChargeRunResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.HireCharges.Post)]
    public async Task<IActionResult> Post([FromBody] PostHireChargeRunDto dto)
    {
        var result = await _mediator.Send(new PostHireChargeRunCommand(dto));
        var message = $"Hire {result.RunNumber} posted for {result.DeploymentCount} deployment(s).";
        if (result.Warning is not null) message += $" {result.Warning}";
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<HireChargeRunResponseDto>.Ok(result, message));
    }

    [HttpPost("{id:guid}/reverse")]
    [RequireClaim(Claims.Accounts.HireCharges.Reverse)]
    public async Task<IActionResult> Reverse(Guid id)
    {
        var result = await _mediator.Send(new ReverseHireChargeRunCommand(id));
        return Ok(ApiResponse<HireChargeRunResponseDto>.Ok(result, $"Hire {result.RunNumber} reversed."));
    }
}
