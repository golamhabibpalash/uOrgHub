using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.Features.Payment;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/payments")]
public class PaymentsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public PaymentsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.Payments.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? customerId, [FromQuery] Guid? vendorId)
    {
        var result = await _mediator.Send(new GetPaymentsQuery(request, customerId, vendorId));
        return Ok(ApiResponse<PagedResult<PaymentResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.Payments.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllPaymentsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, PaymentExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Payments"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Payments.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id));
        return Ok(ApiResponse<PaymentResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Payments.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var result = await _mediator.Send(new CreatePaymentCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentResponseDto>.Ok(result, "Payment recorded successfully."));
    }
}
