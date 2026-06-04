using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers;

[ApiController]
[Route("api/v1/company")]
public class CompanyController : ControllerBase
{
    private readonly AppDbContext _db;

    public CompanyController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var hasCompany = await _db.Set<Company>().AnyAsync(c => !c.IsDeleted);
        var hasUsers = await _db.Set<ApplicationUser>().AnyAsync(u => !u.IsDeleted);
        return Ok(ApiResponse<CompanyStatusDto>.Ok(new CompanyStatusDto(hasCompany, hasUsers)));
    }

    [HttpPost("setup")]
    public async Task<IActionResult> Setup([FromBody] CompanySetupDto dto)
    {
        var hasCompany = await _db.Set<Company>().AnyAsync(c => !c.IsDeleted);
        if (hasCompany)
            throw new AppException("Company already configured.");

        var company = new Company
        {
            Name = dto.CompanyName,
            TagLine = dto.TagLine,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            TaxId = dto.TaxId,
            Currency = dto.Currency ?? "BDT",
            TimeZone = dto.TimeZone ?? "Asia/Dhaka",
            LogoUrl = dto.LogoUrl,
        };

        _db.Set<Company>().Add(company);

        var user = new ApplicationUser
        {
            Username = dto.AdminUsername,
            Email = dto.AdminEmail,
            FirstName = dto.AdminFirstName,
            LastName = dto.AdminLastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword, workFactor: 12),
            MustChangePassword = true,
            CreatedBy = "System",
        };

        _db.Set<ApplicationUser>().Add(user);
        await _db.SaveChangesAsync();

        var adminRole = await _db.Set<ApplicationRole>().FirstOrDefaultAsync(r => r.Name == "Admin" && !r.IsDeleted);
        if (adminRole != null)
        {
            _db.Set<UserRole>().Add(new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                AssignedBy = "System",
                AssignedAt = DateTime.UtcNow,
            });
        }

        _db.Set<UserCompany>().Add(new UserCompany
        {
            UserId = user.Id,
            CompanyId = company.Id,
            IsDefault = true,
            RoleInCompany = "SuperAdmin",
            AssignedBy = "System",
            AssignedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<CompanySetupResultDto>.Ok(new CompanySetupResultDto(company.Id, company.Name)));
    }

    [HttpGet]
    [Authorize]
    [RequireClaim(Claims.Admin.Company.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var query = _db.Set<Company>().Where(c => !c.IsDeleted);
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = items.Select(c => new CompanyDto(
            c.Id, c.Name, c.TagLine, c.Address, c.Phone, c.Email, c.TaxId,
            c.LogoUrl, c.Currency, c.TimeZone, c.IsActive,
            c.CreatedAt, c.UpdatedAt
        )).ToList();

        return Ok(ApiResponse<PagedResult<CompanyDto>>.Ok(new PagedResult<CompanyDto>
        {
            Items = dtos, TotalCount = total, Page = request.Page, PageSize = request.PageSize,
        }));
    }

    [HttpGet("mine")]
    [Authorize]
    [RequireClaim(Claims.Admin.Company.View)]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();
        var company = await _db.Set<UserCompany>()
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId && !uc.IsDeleted)
            .Select(uc => uc.Company)
            .FirstOrDefaultAsync(c => c != null && !c.IsDeleted)
            ?? throw new AppException("No company found for current user.");

        return Ok(ApiResponse<CompanyDto>.Ok(MapDto(company)));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [RequireClaim(Claims.Admin.Company.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var company = await _db.Set<Company>().FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
            ?? throw new NotFoundException("Company", id);

        return Ok(ApiResponse<CompanyDto>.Ok(MapDto(company)));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [RequireClaim(Claims.Admin.Company.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        var company = await _db.Set<Company>().FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
            ?? throw new NotFoundException("Company", id);

        if (dto.Name != null) company.Name = dto.Name;
        if (dto.TagLine != null) company.TagLine = dto.TagLine;
        if (dto.Address != null) company.Address = dto.Address;
        if (dto.Phone != null) company.Phone = dto.Phone;
        if (dto.Email != null) company.Email = dto.Email;
        if (dto.TaxId != null) company.TaxId = dto.TaxId;
        if (dto.LogoUrl != null) company.LogoUrl = dto.LogoUrl;
        if (dto.Currency != null) company.Currency = dto.Currency;
        if (dto.TimeZone != null) company.TimeZone = dto.TimeZone;
        if (dto.IsActive.HasValue) company.IsActive = dto.IsActive.Value;
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = GetUserId().ToString();

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<CompanyDto>.Ok(MapDto(company)));
    }

    [HttpPost("{id:guid}/logo")]
    [Authorize]
    [RequireClaim(Claims.Admin.Company.Edit)]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
    {
        var company = await _db.Set<Company>().FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
            ?? throw new NotFoundException("Company", id);

        if (file == null || file.Length == 0)
            throw new AppException("No file provided.");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "companies", id.ToString());
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"logo{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        company.LogoUrl = $"/uploads/companies/{id}/logo{ext}";
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = GetUserId().ToString();
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok(company.LogoUrl));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [RequireClaim(Claims.Admin.Company.Edit)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var company = await _db.Set<Company>().FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
            ?? throw new NotFoundException("Company", id);

        company.IsDeleted = true;
        company.DeletedAt = DateTime.UtcNow;
        company.DeletedBy = GetUserId().ToString();
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Company deleted."));
    }

    private static CompanyDto MapDto(Company c) => new(
        c.Id, c.Name, c.TagLine, c.Address, c.Phone, c.Email, c.TaxId,
        c.LogoUrl, c.Currency, c.TimeZone, c.IsActive,
        c.CreatedAt, c.UpdatedAt
    );
}

public record CompanyStatusDto(bool HasCompany, bool HasUsers);

public record CompanySetupDto(
    string CompanyName,
    string? TagLine,
    string? Address,
    string? Phone,
    string? Email,
    string? TaxId,
    string? LogoUrl,
    string? Currency,
    string? TimeZone,
    string AdminUsername,
    string AdminEmail,
    string AdminFirstName,
    string AdminLastName,
    string AdminPassword
);

public record CompanyDto(
    Guid Id, string Name, string? TagLine, string? Address, string? Phone,
    string? Email, string? TaxId, string? LogoUrl, string Currency,
    string TimeZone, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt
);

public record UpdateCompanyDto(
    string? Name, string? TagLine, string? Address, string? Phone,
    string? Email, string? TaxId, string? LogoUrl, string? Currency,
    string? TimeZone, bool? IsActive
);

public record CompanySetupResultDto(Guid CompanyId, string CompanyName);
