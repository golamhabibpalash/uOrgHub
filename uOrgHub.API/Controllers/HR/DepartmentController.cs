using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features.CoreHR.Commands;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/departments")]
public class DepartmentController : BaseController
{
    private readonly IMediator _mediator;

    public DepartmentController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.HR.Departments.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetDepartmentsQuery(request));
        return Ok(ApiResponse<PagedResult<DepartmentResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.HR.Departments.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id));
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.HR.Departments.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var result = await _mediator.Send(new CreateDepartmentCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DepartmentResponseDto>.Ok(result, "Department created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.HR.Departments.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _mediator.Send(new UpdateDepartmentCommand(id, dto));
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result, "Department updated successfully."));
    }

    [HttpGet("{id:guid}/dependencies")]
    [RequireClaim(Claims.HR.Departments.Delete)]
    public async Task<IActionResult> GetDependencies(Guid id)
    {
        var result = await _mediator.Send(new GetDepartmentDependenciesQuery(id));
        return Ok(ApiResponse<DepartmentDependenciesDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.HR.Departments.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDepartmentCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Department deleted successfully."));
    }
}
