using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.GoodsReceivedNotes.Commands;
using uOrgHub.Procurement.Features.GoodsReceivedNotes.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class GoodsReceivedNotesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public GoodsReceivedNotesController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Procurement.GRNs.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] GRNStatus? status = null)
    {
        var result = await _mediator.Send(new GetGRNsQuery(request, status));
        return Ok(ApiResponse<PagedResult<GRNResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Procurement.GRNs.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] GRNStatus? status = null)
    {
        var data = await _mediator.Send(new GetAllGRNsForExportQuery(status));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, GRNExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "GoodsReceivedNotes"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Procurement.GRNs.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetGRNByIdQuery(id));
        return Ok(ApiResponse<GRNResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Procurement.GRNs.Create)]
    public async Task<IActionResult> Create([FromBody] CreateGRNDto dto)
    {
        var result = await _mediator.Send(new CreateGRNCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<GRNResponseDto>.Ok(result, "GRN created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Procurement.GRNs.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGRNDto dto)
    {
        var result = await _mediator.Send(new UpdateGRNCommand(id, dto));
        return Ok(ApiResponse<GRNResponseDto>.Ok(result, "GRN updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Procurement.GRNs.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteGRNCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "GRN deleted successfully."));
    }

    [HttpPost("{id:guid}/confirm")]
    [RequireClaim(Claims.Procurement.GRNs.Edit)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmGRNCommand(id));
        return Ok(ApiResponse<GRNResponseDto>.Ok(result, "GRN confirmed. Stock balances updated."));
    }
}
