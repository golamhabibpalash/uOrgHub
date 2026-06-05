using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Banking;
using uOrgHub.Accounts.Features.Banking;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/bank-accounts")]
public class BankAccountsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public BankAccountsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.BankAccounts.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetBankAccountsQuery(request));
        return Ok(ApiResponse<PagedResult<BankAccountResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.BankAccounts.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllBankAccountsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, BankAccountExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "BankAccounts"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.BankAccounts.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBankAccountByIdQuery(id));
        return Ok(ApiResponse<BankAccountResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.BankAccounts.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountDto dto)
    {
        var result = await _mediator.Send(new CreateBankAccountCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BankAccountResponseDto>.Ok(result, "Bank account created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.BankAccounts.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankAccountDto dto)
    {
        var result = await _mediator.Send(new UpdateBankAccountCommand(id, dto));
        return Ok(ApiResponse<BankAccountResponseDto>.Ok(result, "Bank account updated successfully."));
    }

    [HttpGet("{id:guid}/transactions")]
    [RequireClaim(Claims.Accounts.BankAccounts.View)]
    public async Task<IActionResult> GetTransactions(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetBankTransactionsQuery(id, request));
        return Ok(ApiResponse<PagedResult<BankTransactionResponseDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/transactions")]
    [RequireClaim(Claims.Accounts.BankAccounts.Edit)]
    public async Task<IActionResult> CreateTransaction(Guid id, [FromBody] CreateBankTransactionDto dto)
    {
        dto.BankAccountId = id;
        var result = await _mediator.Send(new CreateBankTransactionCommand(dto));
        return Ok(ApiResponse<BankTransactionResponseDto>.Ok(result, "Transaction recorded successfully."));
    }
}
