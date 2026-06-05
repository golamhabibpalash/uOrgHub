using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.DTOs;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features.CoreHR.Commands;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/employees")]
public class EmployeeController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IEmployeeWithUserService _employeeWithUserService;
    private readonly IExportService _exportService;

    public EmployeeController(IMediator mediator, IEmployeeWithUserService employeeWithUserService, IExportService exportService)
    {
        _mediator = mediator;
        _employeeWithUserService = employeeWithUserService;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.HR.Employees.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? departmentId = null, [FromQuery] Guid? designationId = null)
    {
        var result = await _mediator.Send(new GetEmployeesQuery(request, departmentId, designationId));
        return Ok(ApiResponse<PagedResult<EmployeeResponseDto>>.Ok(result));
    }

    [HttpGet("organogram")]
    [RequireClaim(Claims.HR.Employees.View)]
    public async Task<IActionResult> GetOrganogram([FromQuery] Guid? departmentId = null, [FromQuery] Guid? designationId = null)
    {
        var result = await _mediator.Send(new GetOrganogramQuery(departmentId, designationId));
        return Ok(ApiResponse<List<OrganogramNodeDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.HR.Employees.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] Guid? departmentId = null, [FromQuery] Guid? designationId = null, [FromQuery] string? search = null)
    {
        var data = await _mediator.Send(new GetAllEmployeesQuery(departmentId, designationId, search));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, EmployeeExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Employees"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.HR.Employees.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(id));
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.HR.Employees.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _mediator.Send(new CreateEmployeeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<EmployeeResponseDto>.Ok(result, "Employee created successfully."));
    }

    [HttpPost("with-user")]
    [RequireClaim(Claims.HR.Employees.Create)]
    public async Task<IActionResult> CreateWithUser([FromBody] CreateEmployeeWithUserDto dto)
    {
        var result = await _employeeWithUserService.CreateEmployeeWithUserAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<EmployeeResponseDto>.Ok(result, "Employee and user account created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.HR.Employees.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await _mediator.Send(new UpdateEmployeeCommand(id, dto));
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result, "Employee updated successfully."));
    }

    [HttpGet("{id:guid}/dependencies")]
    [RequireClaim(Claims.HR.Employees.Delete)]
    public async Task<IActionResult> GetDependencies(Guid id)
    {
        var result = await _mediator.Send(new GetEmployeeDependenciesQuery(id));
        return Ok(ApiResponse<EmployeeDependenciesDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.HR.Employees.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteEmployeeCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Employee deleted successfully."));
    }
}
