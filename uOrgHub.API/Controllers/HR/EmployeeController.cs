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
    private readonly EmployeeProfilePictureService _pictureService;

    public EmployeeController(
        IMediator mediator,
        IEmployeeWithUserService employeeWithUserService,
        IExportService exportService,
        EmployeeProfilePictureService pictureService)
    {
        _mediator = mediator;
        _employeeWithUserService = employeeWithUserService;
        _exportService = exportService;
        _pictureService = pictureService;
    }

    [HttpGet]
    [RequireClaim(Claims.HR.Employees.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? departmentId = null, [FromQuery] Guid? designationId = null)
    {
        var result = await _mediator.Send(new GetEmployeesQuery(request, departmentId, designationId));
        return Ok(ApiResponse<PagedResult<EmployeeResponseDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllForDropdown()
    {
        var result = await _mediator.Send(new GetAllEmployeesQuery());
        return Ok(ApiResponse<List<EmployeeResponseDto>>.Ok(result));
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

    [HttpPost("{id:guid}/profile-picture")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequireClaim(Claims.HR.Employees.Edit)]
    public async Task<IActionResult> UploadProfilePicture(Guid id, IFormFile file, CancellationToken ct)
    {
        var url = await _pictureService.UploadForEmployeeAsync(id, file, ct);
        return Ok(ApiResponse<string>.Ok(url, "Profile picture uploaded successfully."));
    }

    [HttpDelete("{id:guid}/profile-picture")]
    [RequireClaim(Claims.HR.Employees.Edit)]
    public async Task<IActionResult> DeleteProfilePicture(Guid id, CancellationToken ct)
    {
        await _pictureService.DeleteForEmployeeAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Removed", "Profile picture removed successfully."));
    }

    [HttpPost("{id:guid}/nid-photo")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequireClaim(Claims.HR.Employees.Edit)]
    public async Task<IActionResult> UploadNidPhoto(Guid id, IFormFile file, CancellationToken ct)
    {
        var url = await _pictureService.UploadNidPhotoForEmployeeAsync(id, file, ct);
        return Ok(ApiResponse<string>.Ok(url, "NID photo uploaded successfully."));
    }

    [HttpDelete("{id:guid}/nid-photo")]
    [RequireClaim(Claims.HR.Employees.Edit)]
    public async Task<IActionResult> DeleteNidPhoto(Guid id, CancellationToken ct)
    {
        await _pictureService.DeleteNidPhotoForEmployeeAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Removed", "NID photo removed successfully."));
    }
}
