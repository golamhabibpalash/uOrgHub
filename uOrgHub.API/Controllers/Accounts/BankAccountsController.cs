using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Banking;
using uOrgHub.Accounts.Features.Banking;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
public class BankAccountsController : BaseController
{
    private readonly IMediator _mediator;
    public BankAccountsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetBankAccountsQuery(request));
        return Ok(ApiResponse<PagedResult<BankAccountResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBankAccountByIdQuery(id));
        return Ok(ApiResponse<BankAccountResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountDto dto)
    {
        var result = await _mediator.Send(new CreateBankAccountCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BankAccountResponseDto>.Ok(result, "Bank account created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankAccountDto dto)
    {
        var result = await _mediator.Send(new UpdateBankAccountCommand(id, dto));
        return Ok(ApiResponse<BankAccountResponseDto>.Ok(result, "Bank account updated successfully."));
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetBankTransactionsQuery(id, request));
        return Ok(ApiResponse<PagedResult<BankTransactionResponseDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/transactions")]
    public async Task<IActionResult> CreateTransaction(Guid id, [FromBody] CreateBankTransactionDto dto)
    {
        dto.BankAccountId = id;
        var result = await _mediator.Send(new CreateBankTransactionCommand(dto));
        return Ok(ApiResponse<BankTransactionResponseDto>.Ok(result, "Transaction recorded successfully."));
    }
}
