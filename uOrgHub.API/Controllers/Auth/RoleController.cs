using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly AppDbContext _db;

    public RoleController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Set<ApplicationRole>()
            .Include(r => r.UserRoles)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id, r.Name, r.Description, r.IsSystem, r.IsActive,
                r.UserRoles.Count,
                null
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<RoleDto>>.Ok(roles));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var role = await _db.Set<ApplicationRole>()
            .Include(r => r.UserRoles)
            .Include(r => r.RoleClaims).ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted)
            ?? throw new NotFoundException(nameof(ApplicationRole), id);

        var dto = new RoleDto(
            role.Id, role.Name, role.Description, role.IsSystem, role.IsActive,
            role.UserRoles.Count,
            role.RoleClaims.Where(rc => !rc.Claim.IsDeleted).Select(rc => new ClaimDto(
                rc.Claim.Id, rc.Claim.Name, rc.Claim.Description, rc.Claim.Module, rc.Claim.Category, rc.Claim.IsActive
            )).ToList()
        );

        return Ok(ApiResponse<RoleDto>.Ok(dto));
    }

    [HttpPost]
    [RequireClaim("Users.Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        var role = new ApplicationRole
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedBy = "system",
        };

        _db.Set<ApplicationRole>().Add(role);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, ApiResponse<RoleDto>.Ok(
            new RoleDto(role.Id, role.Name, role.Description, role.IsSystem, role.IsActive, 0, null),
            "Role created"
        ));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim("Users.Edit")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _db.Set<ApplicationRole>().FirstAsync(r => r.Id == id);
        role.Name = dto.Name;
        role.Description = dto.Description;
        role.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Role updated"));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim("Users.Delete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _db.Set<ApplicationRole>().FirstAsync(r => r.Id == id);

        if (role.IsSystem)
            throw new AppException("System roles cannot be deleted.");

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Role deleted"));
    }

    [HttpGet("{id:guid}/claims")]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetRoleClaims(Guid id)
    {
        var claims = await _db.Set<RoleClaim>()
            .Where(rc => rc.RoleId == id && !rc.Claim.IsDeleted)
            .Select(rc => new ClaimDto(
                rc.Claim.Id, rc.Claim.Name, rc.Claim.Description, rc.Claim.Module, rc.Claim.Category, rc.Claim.IsActive
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<ClaimDto>>.Ok(claims));
    }

    [HttpPost("{id:guid}/claims")]
    [RequireClaim("Users.AssignRoles")]
    public async Task<IActionResult> AssignClaimToRole(Guid id, [FromBody] AssignRoleClaimsDto dto)
    {
        var existing = _db.Set<RoleClaim>().Where(rc => rc.RoleId == id);
        _db.Set<RoleClaim>().RemoveRange(existing);

        foreach (var claimId in dto.ClaimIds)
        {
            _db.Set<RoleClaim>().Add(new RoleClaim
            {
                RoleId = id,
                ClaimId = claimId,
                AssignedBy = "system",
                AssignedAt = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Claims assigned to role"));
    }

    [HttpDelete("{id:guid}/claims/{claimId:guid}")]
    [RequireClaim("Users.AssignRoles")]
    public async Task<IActionResult> RemoveClaimFromRole(Guid id, Guid claimId)
    {
        var roleClaim = await _db.Set<RoleClaim>()
            .FirstOrDefaultAsync(rc => rc.RoleId == id && rc.ClaimId == claimId);

        if (roleClaim != null)
        {
            _db.Set<RoleClaim>().Remove(roleClaim);
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<string>.Ok("Claim removed from role"));
    }
}
