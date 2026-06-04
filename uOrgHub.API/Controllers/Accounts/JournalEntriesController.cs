using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/journal-entries")]
public class JournalEntriesController : BaseController
{
    private readonly IJournalEntryService _service;

    public JournalEntriesController(IJournalEntryService service) => _service = service;

    [HttpGet]
    [RequireClaim(Claims.Accounts.JournalEntries.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<JournalEntryResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.JournalEntries.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<JournalEntryResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.JournalEntries.Create)]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<JournalEntryResponseDto>.Ok(result, "Journal entry created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.JournalEntries.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJournalEntryDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<JournalEntryResponseDto>.Ok(result, "Journal entry updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.JournalEntries.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Journal entry deleted successfully."));
    }

    [HttpPost("{id:guid}/post")]
    [RequireClaim(Claims.Accounts.JournalEntries.Post)]
    public async Task<IActionResult> Post(Guid id)
    {
        var result = await _service.PostAsync(id, User.Identity?.Name ?? "System");
        return Ok(ApiResponse<JournalEntryResponseDto>.Ok(result, "Journal entry posted successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireClaim(Claims.Accounts.JournalEntries.Delete)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _service.CancelAsync(id);
        return Ok(ApiResponse<JournalEntryResponseDto>.Ok(result, "Journal entry cancelled successfully."));
    }
}
