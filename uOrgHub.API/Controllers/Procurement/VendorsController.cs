using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.Vendors.Commands;
using uOrgHub.Procurement.Features.Vendors.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class VendorsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public VendorsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Procurement.Vendors.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] VendorStatus? status = null, [FromQuery] VendorType? vendorType = null)
    {
        var result = await _mediator.Send(new GetVendorsQuery(request, status, vendorType));
        return Ok(ApiResponse<PagedResult<VendorResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Procurement.Vendors.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] VendorStatus? status = null, [FromQuery] VendorType? vendorType = null, [FromQuery] string? search = null)
    {
        var data = await _mediator.Send(new GetAllVendorsQuery(status, vendorType, search));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, VendorExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Vendors"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Procurement.Vendors.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetVendorByIdQuery(id));
        return Ok(ApiResponse<VendorResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Procurement.Vendors.Create)]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
    {
        var result = await _mediator.Send(new CreateVendorCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<VendorResponseDto>.Ok(result, "Vendor created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Procurement.Vendors.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorDto dto)
    {
        var result = await _mediator.Send(new UpdateVendorCommand(id, dto));
        return Ok(ApiResponse<VendorResponseDto>.Ok(result, "Vendor updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Procurement.Vendors.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteVendorCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Vendor deleted successfully."));
    }

    [HttpGet("{id:guid}/quotations")]
    [RequireClaim(Claims.Procurement.Vendors.View)]
    public async Task<IActionResult> GetQuotations(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetVendorQuotationsQuery(id, request));
        return Ok(ApiResponse<PagedResult<VendorQuotationResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}/orders")]
    [RequireClaim(Claims.Procurement.Vendors.View)]
    public async Task<IActionResult> GetOrders(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetVendorOrdersQuery(id, request));
        return Ok(ApiResponse<PagedResult<POResponseDto>>.Ok(result));
    }
}
