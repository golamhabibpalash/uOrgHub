using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/customers")]
public class CustomersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public CustomersController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.Customers.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetCustomersQuery(request));
        return Ok(ApiResponse<PagedResult<CustomerResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.Customers.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] string? search = null)
    {
        var data = await _mediator.Send(new GetAllCustomersQuery(search));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, CustomerExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Customers"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Customers.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(ApiResponse<CustomerResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Customers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CustomerResponseDto>.Ok(result, "Customer created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.Customers.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(id, dto));
        return Ok(ApiResponse<CustomerResponseDto>.Ok(result, "Customer updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.Customers.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Customer deleted successfully."));
    }
}
