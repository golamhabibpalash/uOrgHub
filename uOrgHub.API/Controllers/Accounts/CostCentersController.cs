using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.CostCenter;
using uOrgHub.Accounts.Features.CostCenter;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/cost-centers")]
public class CostCentersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public CostCentersController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.CostCenters.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetCostCentersQuery(request));
        return Ok(ApiResponse<PagedResult<CostCenterResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.CostCenters.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllCostCentersForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, CostCenterExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "CostCenters"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.CostCenters.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCostCenterByIdQuery(id));
        return Ok(ApiResponse<CostCenterResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.CostCenters.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCostCenterDto dto)
    {
        var result = await _mediator.Send(new CreateCostCenterCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CostCenterResponseDto>.Ok(result, "Cost center created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.CostCenters.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCostCenterDto dto)
    {
        var result = await _mediator.Send(new UpdateCostCenterCommand(id, dto));
        return Ok(ApiResponse<CostCenterResponseDto>.Ok(result, "Cost center updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.CostCenters.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCostCenterCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Cost center deleted successfully."));
    }
}
