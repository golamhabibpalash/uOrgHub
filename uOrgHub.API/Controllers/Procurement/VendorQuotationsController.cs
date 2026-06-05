using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.VendorQuotations.Commands;
using uOrgHub.Procurement.Features.VendorQuotations.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class VendorQuotationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public VendorQuotationsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Procurement.Quotations.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] QuotationStatus? status = null)
    {
        var result = await _mediator.Send(new GetQuotationsQuery(request, status));
        return Ok(ApiResponse<PagedResult<VendorQuotationResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Procurement.Quotations.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] QuotationStatus? status = null)
    {
        var data = await _mediator.Send(new GetAllQuotationsForExportQuery(status));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, QuotationExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "VendorQuotations"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Procurement.Quotations.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetQuotationByIdQuery(id));
        return Ok(ApiResponse<VendorQuotationResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Procurement.Quotations.Create)]
    public async Task<IActionResult> Create([FromBody] CreateVendorQuotationDto dto)
    {
        var result = await _mediator.Send(new CreateVendorQuotationCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<VendorQuotationResponseDto>.Ok(result, "Vendor quotation created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Procurement.Quotations.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorQuotationDto dto)
    {
        var result = await _mediator.Send(new UpdateVendorQuotationCommand(id, dto));
        return Ok(ApiResponse<VendorQuotationResponseDto>.Ok(result, "Vendor quotation updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Procurement.Quotations.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteVendorQuotationCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Vendor quotation deleted successfully."));
    }
}
