using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features.FiscalYear;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/fiscal-years")]
public class FiscalYearsController : BaseController
{
    private readonly IFiscalYearService _service;
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public FiscalYearsController(IFiscalYearService service, IMediator mediator, IExportService exportService)
    {
        _service = service;
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.FiscalYears.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<FiscalYearResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.FiscalYears.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllFiscalYearsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, FiscalYearExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "FiscalYears"
        });
        return File(result.Content, result.MimeType, result.FileName);
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
