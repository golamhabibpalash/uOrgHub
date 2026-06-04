using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/fiscal-years")]
public class FiscalYearsController : BaseController
{
    private readonly IFiscalYearService _service;

    public FiscalYearsController(IFiscalYearService service) => _service = service;

    [HttpGet]
    [RequireClaim(Claims.Accounts.FiscalYears.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<FiscalYearResponseDto>>.Ok(result));
    }

    [HttpGet("current")]
    [RequireClaim(Claims.Accounts.FiscalYears.View)]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await _service.GetCurrentAsync();
        return Ok(ApiResponse<FiscalYearResponseDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.FiscalYears.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<FiscalYearResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.FiscalYears.Create)]
    public async Task<IActionResult> Create([FromBody] CreateFiscalYearDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<FiscalYearResponseDto>.Ok(result, "Fiscal year created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.FiscalYears.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFiscalYearDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<FiscalYearResponseDto>.Ok(result, "Fiscal year updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.FiscalYears.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Fiscal year deleted successfully."));
    }
}
