using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.PurchaseOrders.Commands;
using uOrgHub.Procurement.Features.PurchaseOrders.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class PurchaseOrdersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public PurchaseOrdersController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Procurement.PurchaseOrders.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] POStatus? status = null)
    {
        var result = await _mediator.Send(new GetPOsQuery(request, status));
        return Ok(ApiResponse<PagedResult<POResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] POStatus? status = null)
    {
        var data = await _mediator.Send(new GetAllPOsForExportQuery(status));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, POExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "PurchaseOrders"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPOByIdQuery(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var result = await _mediator.Send(new CreatePOCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<POResponseDto>.Ok(result, "Purchase order created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        var result = await _mediator.Send(new UpdatePOCommand(id, dto));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePOCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Purchase order deleted successfully."));
    }

    [HttpPost("{id:guid}/send")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Edit)]
    public async Task<IActionResult> Send(Guid id)
    {
        var result = await _mediator.Send(new SendPOCommand(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order sent to vendor."));
    }

    [HttpPost("{id:guid}/confirm")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Approve)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmPOCommand(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order confirmed."));
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.Edit)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelPOCommand(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order cancelled."));
    }

    [HttpGet("{id:guid}/grns")]
    [RequireClaim(Claims.Procurement.PurchaseOrders.View)]
    public async Task<IActionResult> GetGRNs(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetPOGRNsQuery(id, request));
        return Ok(ApiResponse<PagedResult<GRNResponseDto>>.Ok(result));
    }
}
