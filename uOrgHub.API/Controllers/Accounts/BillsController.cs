using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.Features.AP;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/bills")]
public class BillsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public BillsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.Bills.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? vendorId, [FromQuery] BillStatus? status)
    {
        var result = await _mediator.Send(new GetBillsQuery(request, vendorId, status));
        return Ok(ApiResponse<PagedResult<BillResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.Bills.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllBillsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, BillExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Bills"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Bills.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBillByIdQuery(id));
        return Ok(ApiResponse<BillResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Bills.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBillDto dto)
    {
        var result = await _mediator.Send(new CreateBillCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BillResponseDto>.Ok(result, "Bill created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.Bills.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBillDto dto)
    {
        var result = await _mediator.Send(new UpdateBillCommand(id, dto));
        return Ok(ApiResponse<BillResponseDto>.Ok(result, "Bill updated successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Accounts.Bills.Approve)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveBillCommand(id));
        return Ok(ApiResponse<BillResponseDto>.Ok(result, "Bill approved successfully."));
    }

    [HttpPost("{id:guid}/void")]
    [RequireClaim(Claims.Accounts.Bills.Delete)]
    public async Task<IActionResult> Void(Guid id)
    {
        var result = await _mediator.Send(new VoidBillCommand(id));
        return Ok(ApiResponse<BillResponseDto>.Ok(result, "Bill voided successfully."));
    }
}
