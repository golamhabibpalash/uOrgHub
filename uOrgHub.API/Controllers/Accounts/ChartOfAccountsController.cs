using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/chart-of-accounts")]
public class ChartOfAccountsController : BaseController
{
    private readonly IChartOfAccountService _service;

    public ChartOfAccountsController(IChartOfAccountService service) => _service = service;

    [HttpGet]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<ChartOfAccountResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ChartOfAccountResponseDto>.Ok(result));
    }

    [HttpGet("{id:guid}/ledger")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GetLedger(Guid id)
    {
        var result = await _service.GetLedgerAsync(id);
        return Ok(ApiResponse<List<JournalEntryLineResponseDto>>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Create)]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ChartOfAccountResponseDto>.Ok(result, "Chart of account created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChartOfAccountDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<ChartOfAccountResponseDto>.Ok(result, "Chart of account updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Chart of account deleted successfully."));
    }
}
