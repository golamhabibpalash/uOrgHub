using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<EmployeeResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<EmployeeResponseDto>.Ok(result, "Employee created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result, "Employee updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Employee deleted successfully."));
    }
}
