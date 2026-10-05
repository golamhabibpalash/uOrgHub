using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.Features.Payment;
using uOrgHub.Accounts.Models.Enums;
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

    /// <summary>The MR No. the next money receipt will get.</summary>
    [HttpGet("mr-series")]
    [RequireClaim(Claims.Accounts.Payments.View)]
    public async Task<IActionResult> GetMoneyReceiptSeries()
        => Ok(ApiResponse<MoneyReceiptSeriesDto>.Ok(await _mediator.Send(new GetMoneyReceiptSeriesQuery())));

    /// <summary>Sets where the MR No. series continues (e.g. after the last paper receipt).</summary>
    [HttpPut("mr-series")]
    [RequireClaim(Claims.Accounts.Payments.Edit)]
    public async Task<IActionResult> SetMoneyReceiptSeries([FromBody] MoneyReceiptSeriesDto dto)
    {
        var result = await _mediator.Send(new SetMoneyReceiptSeriesCommand(dto));
        return Ok(ApiResponse<MoneyReceiptSeriesDto>.Ok(result, $"Next MR No. set to {result.NextNumber}."));
    }

    /// <summary>Data for the printable money receipt of a payment received.</summary>
    [HttpGet("{id:guid}/receipt")]
    [RequireClaim(Claims.Accounts.Payments.View)]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentReceiptQuery(id));

        // Invoices raised from RA bills / retention releases carry that origin on the receipt.
        var invoiceIds = result.Lines.Where(l => l.DocumentType == "Invoice").Select(l => l.DocumentId).ToList();
        var sources = await _mediator.Send(new uOrgHub.Projects.Features.RABills.Queries.GetInvoiceSourcesQuery(invoiceIds));
        foreach (var line in result.Lines)
            if (sources.TryGetValue(line.DocumentId, out var source))
                line.SourceReference = source;

        return Ok(ApiResponse<PaymentReceiptDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Payments.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var result = await _mediator.Send(new CreatePaymentCommand(dto, GetUserName()));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentResponseDto>.Ok(result, "Payment recorded successfully."));
    }

    [HttpPost("{id:guid}/void")]
    [RequireClaim(Claims.Accounts.Payments.Delete)]
    public async Task<IActionResult> Void(Guid id)
    {
        var result = await _mediator.Send(new VoidPaymentCommand(id));
        return Ok(ApiResponse<PaymentResponseDto>.Ok(result, "Payment voided successfully."));
    }
}
