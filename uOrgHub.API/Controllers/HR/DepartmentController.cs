using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
public class DepartmentController : BaseController
{
    private readonly IDepartmentService _service;

    public DepartmentController(IDepartmentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<DepartmentResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DepartmentResponseDto>.Ok(result, "Department created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result, "Department updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Department deleted successfully."));
    }
}
