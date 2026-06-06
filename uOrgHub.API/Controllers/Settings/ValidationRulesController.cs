using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;
using uOrgHub.Settings.DTOs;
using uOrgHub.Settings.Services;

namespace uOrgHub.API.Controllers.Settings;

[Authorize]
[Route("api/v1/validation-rules")]
public class ValidationRulesController : BaseController
{
    private readonly IValidationRuleService _service;

    public ValidationRulesController(IValidationRuleService service) => _service = service;

    [HttpGet]
    [RequireClaim(Claims.Settings.ValidationRules.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<ValidationRuleResponseDto>>.Ok(result));
    }

    [HttpGet("by-entity/{entityType}")]
    [RequireClaim(Claims.Settings.ValidationRules.View)]
    public async Task<IActionResult> GetByEntityType(string entityType)
    {
        var result = await _service.GetByEntityTypeAsync(entityType);
        return Ok(ApiResponse<List<ValidationRuleResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Settings.ValidationRules.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ValidationRuleResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Settings.ValidationRules.Create)]
    public async Task<IActionResult> Create([FromBody] CreateValidationRuleDto dto)
    {
        var result = await _service.CreateAsync(dto, GetUserName());
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ValidationRuleResponseDto>.Ok(result, "Rule created."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Settings.ValidationRules.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateValidationRuleDto dto)
    {
        var result = await _service.UpdateAsync(id, dto, GetUserName());
        return Ok(ApiResponse<ValidationRuleResponseDto>.Ok(result, "Rule updated."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Settings.ValidationRules.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, GetUserName());
        return Ok(ApiResponse<string>.Ok("Rule deleted."));
    }
}
