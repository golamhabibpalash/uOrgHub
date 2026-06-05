using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features.JournalEntry;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/journal-entries")]
public class JournalEntriesController : BaseController
{
    private readonly IJournalEntryService _service;
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public JournalEntriesController(IJournalEntryService service, IMediator mediator, IExportService exportService)
    {
        _service = service;
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.JournalEntries.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<JournalEntryResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.JournalEntries.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllJournalEntriesForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, JournalEntryExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "JournalEntries"
        });
        return File(result.Content, result.MimeType, result.FileName);
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
