using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Reporting.ExportColumns;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagement;
    private readonly IAuthService _authService;
    private readonly IAccessLogService _accessLogService;
    private readonly IExportService _exportService;

    public UserManagementController(IUserManagementService userManagement, IAuthService authService, IAccessLogService accessLogService, IExportService exportService)
    {
        _userManagement = userManagement;
        _authService = authService;
        _accessLogService = accessLogService;
        _exportService = exportService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string GetCurrentUser() => User.FindFirstValue("username") ?? "system";

    [HttpGet]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationRequest request)
    {
        var result = await _userManagement.GetUsersAsync(request);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Admin.Users.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _userManagement.GetAllUsersExportAsync();
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, UserExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Users"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userManagement.GetUserByIdAsync(id);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim("Users.Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = await _userManagement.CreateUserAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse<UserDto>.Ok(user, "User created successfully"));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        await _userManagement.UpdateUserAsync(id, dto, GetCurrentUser());
        return Ok(ApiResponse<string>.Ok("User updated successfully"));
    }

    [HttpPut("{id:guid}/username")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> ChangeUsername(Guid id, [FromBody] ChangeUsernameDto dto)
    {
        var updated = await _userManagement.ChangeUsernameAsync(id, dto.NewUsername, GetCurrentUser());
        return Ok(ApiResponse<UserDto>.Ok(updated, "Username changed. The user has been signed out everywhere."));
    }

    [HttpPut("{id:guid}/activate")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        await _userManagement.SetUserActiveAsync(id, true, GetCurrentUser());
        return Ok(ApiResponse<string>.Ok("User activated"));
    }

    [HttpPut("{id:guid}/deactivate")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await _userManagement.SetUserActiveAsync(id, false, GetCurrentUser());
        return Ok(ApiResponse<string>.Ok("User deactivated"));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim("Users.Delete")]
    public async Task<IActionResult> SoftDeleteUser(Guid id)
    {
        await _userManagement.SoftDeleteUserAsync(id, GetCurrentUser());
        return Ok(ApiResponse<string>.Ok("User deleted"));
    }

    [HttpPut("{id:guid}/unlock")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> UnlockUser(Guid id)
    {
        await _userManagement.UnlockUserAsync(id);
        return Ok(ApiResponse<string>.Ok("User unlocked"));
    }

    [HttpPost("{id:guid}/force-logout")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> ForceLogout(Guid id)
    {
        await _userManagement.ForceLogoutUserAsync(id, "AdminForced");
        return Ok(ApiResponse<string>.Ok("User logged out from all sessions"));
    }

    [HttpGet("{id:guid}/sessions")]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetUserSessions(Guid id)
    {
        var sessions = await _userManagement.GetUserSessionsAsync(id);
        return Ok(ApiResponse<List<UserSessionDto>>.Ok(sessions));
    }

    [HttpGet("{id:guid}/access-logs")]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetUserAccessLogs(Guid id, [FromQuery] AccessLogFilterRequest request)
    {
        var logs = await _userManagement.GetUserAccessLogsAsync(id, request);
        return Ok(ApiResponse<List<UserAccessLogDto>>.Ok(logs));
    }

    [HttpPost("{id:guid}/roles")]
    [RequireClaim("Users.AssignRoles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleDto dto)
    {
        await _userManagement.ReplaceRolesAsync(id, dto.RoleIds, GetCurrentUser());
        return Ok(ApiResponse<string>.Ok("Roles assigned"));
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [RequireClaim("Users.AssignRoles")]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
    {
        await _userManagement.RemoveRoleAsync(id, roleId);
        return Ok(ApiResponse<string>.Ok("Role removed"));
    }

    [HttpPost("{id:guid}/claims")]
    [RequireClaim("Users.AssignRoles")]
    public async Task<IActionResult> AssignUserClaim(Guid id, [FromBody] AssignUserClaimDto dto)
    {
        await _userManagement.AssignUserClaimAsync(id, dto.ClaimId, dto.IsGranted, GetCurrentUser());
        return Ok(ApiResponse<string>.Ok(dto.IsGranted ? "Claim granted" : "Claim denied"));
    }

    [HttpDelete("{id:guid}/claims/{claimId:guid}")]
    [RequireClaim("Users.AssignRoles")]
    public async Task<IActionResult> RemoveUserClaim(Guid id, Guid claimId)
    {
        await _userManagement.RemoveUserClaimAsync(id, claimId);
        return Ok(ApiResponse<string>.Ok("Claim removed"));
    }
}
