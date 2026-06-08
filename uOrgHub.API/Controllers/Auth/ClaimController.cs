using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[ApiController]
[Route("api/v1/claims")]
[Authorize]
public class ClaimController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClaimController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [RequireClaim("Users.View")]
    public async Task<IActionResult> GetAllClaims([FromQuery] string? module)
    {
        var query = _db.Set<ApplicationClaim>().Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(c => c.Module == module);

        var claims = await query
            .OrderBy(c => c.Module)
            .ThenBy(c => c.Name)
            .Select(c => new ClaimDto(c.Id, c.Name, c.Description, c.Module, c.Category, c.IsActive))
            .ToListAsync();

        claims = claims.DistinctBy(c => c.Name).ToList();

        return Ok(ApiResponse<List<ClaimDto>>.Ok(claims));
    }
}
