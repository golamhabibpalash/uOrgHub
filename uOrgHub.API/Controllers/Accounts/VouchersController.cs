using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Features.Voucher;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/vouchers")]
public class VouchersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public VouchersController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.Vouchers.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequest request,
        [FromQuery] VoucherType? type,
        [FromQuery] VoucherStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? costCenterId)
    {
        var result = await _mediator.Send(new GetVouchersQuery(request, type, status, fromDate, toDate, accountId, projectId, costCenterId));
        return Ok(ApiResponse<PagedResult<VoucherResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.Vouchers.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllVouchersForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, VoucherExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Vouchers"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    /// <summary>
    /// The accounts valid for each side of the given voucher type, so the form only ever offers
    /// what the server will accept.
    /// </summary>
    [HttpGet("account-options")]
    [RequireClaim(Claims.Accounts.Vouchers.View)]
    public async Task<IActionResult> GetAccountOptions([FromQuery] VoucherType type)
    {
        var result = await _mediator.Send(new GetVoucherAccountOptionsQuery(type));
        return Ok(ApiResponse<VoucherAccountOptionsDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Vouchers.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetVoucherByIdQuery(id));
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result));
    }

    [HttpGet("{id:guid}/journal-entry")]
    [RequireClaim(Claims.Accounts.JournalEntries.View)]
    public async Task<IActionResult> GetJournalEntry(Guid id)
    {
        var result = await _mediator.Send(new GetVoucherJournalEntryQuery(id));
        return Ok(ApiResponse<JournalEntryResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Vouchers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
    {
        var result = await _mediator.Send(new CreateVoucherCommand(dto, GetUserName()));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<VoucherResponseDto>.Ok(result, $"Voucher {result.VoucherNumber} created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.Vouchers.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherDto dto)
    {
        var result = await _mediator.Send(new UpdateVoucherCommand(id, dto, GetUserName()));
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result, "Voucher updated successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    [RequireClaim(Claims.Accounts.Vouchers.Submit)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitVoucherCommand(id, GetUserName()));
        var message = result.JournalEntryNumber is null
            ? "Voucher submitted successfully."
            : $"Voucher submitted. Journal Entry {result.JournalEntryNumber} created as Draft.";
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result, message));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Accounts.Vouchers.Approve)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveVoucherCommand(id, GetUserName()));
        var jeRef = result.JournalEntryNumber;
        var message = jeRef is null
            ? "Voucher approved successfully."
            : $"Voucher approved. Journal Entry {jeRef} is ready to post.";
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result, message));
    }

    [HttpPost("{id:guid}/post")]
    [RequireClaim(Claims.Accounts.Vouchers.Post)]
    public async Task<IActionResult> Post(Guid id)
    {
        var result = await _mediator.Send(new PostVoucherCommand(id, GetUserName()));
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result, $"Voucher posted. Journal Entry {result.JournalEntryNumber} posted successfully."));
    }

    [HttpPost("{id:guid}/reject")]
    [RequireClaim(Claims.Accounts.Vouchers.Reject)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectVoucherDto dto)
    {
        var result = await _mediator.Send(new RejectVoucherCommand(id, dto.Reason, GetUserName()));
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result, "Voucher rejected."));
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireClaim(Claims.Accounts.Vouchers.Delete)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelVoucherCommand(id));
        return Ok(ApiResponse<VoucherResponseDto>.Ok(result, "Voucher cancelled successfully."));
    }
}