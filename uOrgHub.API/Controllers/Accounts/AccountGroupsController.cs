using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features.AccountGroup;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/account-groups")]
public class AccountGroupsController : BaseController
{
    private readonly IAccountGroupService _service;
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public AccountGroupsController(IAccountGroupService service, IMediator mediator, IExportService exportService)
    {
        _service = service;
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.AccountGroups.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<AccountGroupResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.AccountGroups.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllAccountGroupsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, AccountGroupExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "AccountGroups"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.AccountGroups.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<AccountGroupResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.AccountGroups.Create)]
    public async Task<IActionResult> Create([FromBody] CreateAccountGroupDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AccountGroupResponseDto>.Ok(result, "Account group created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.AccountGroups.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountGroupDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<AccountGroupResponseDto>.Ok(result, "Account group updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.AccountGroups.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Account group deleted successfully."));
    }
}
