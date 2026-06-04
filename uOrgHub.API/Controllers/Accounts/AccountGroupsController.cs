using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/account-groups")]
public class AccountGroupsController : BaseController
{
    private readonly IAccountGroupService _service;

    public AccountGroupsController(IAccountGroupService service) => _service = service;

    [HttpGet]
    [RequireClaim(Claims.Accounts.AccountGroups.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<AccountGroupResponseDto>>.Ok(result));
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
