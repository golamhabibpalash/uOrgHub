using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Services;

public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _db;
    private readonly IPermissionService _permissions;
    private readonly IEmailService _email;

    public UserManagementService(AppDbContext db, IPermissionService permissions, IEmailService email)
    {
        _db = db;
        _permissions = permissions;
        _email = email;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, string createdBy)
    {
        var user = new ApplicationUser
        {
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmployeeId = dto.EmployeeId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            IsTwoFactorEnabled = dto.IsTwoFactorEnabled,
            CreatedBy = createdBy,
            MustChangePassword = true,
        };

        _db.Set<ApplicationUser>().Add(user);
        await _db.SaveChangesAsync();

        foreach (var roleId in dto.RoleIds)
        {
            _db.Set<UserRole>().Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedBy = createdBy,
                AssignedAt = DateTime.UtcNow,
            });
        }
        await _db.SaveChangesAsync();

        await _email.SendAsync(user.Email, "Welcome to uOrgHub ERP",
            $"<p>Your account has been created.</p><p>Username: {user.Username}</p><p>Password: {dto.Password}</p><p>Please change your password on first login.</p>");

        return await GetUserByIdAsync(user.Id);
    }

    public async Task SetUserActiveAsync(Guid userId, bool isActive, string updatedBy)
    {
        var user = await _db.Set<ApplicationUser>().FirstAsync(u => u.Id == userId);
        user.IsActive = isActive;
        user.UpdatedBy = updatedBy;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteUserAsync(Guid userId, string deletedBy)
    {
        var user = await _db.Set<ApplicationUser>().FirstAsync(u => u.Id == userId);
        user.IsDeleted = true;
        user.IsActive = false;
        user.Username = $"deleted_{user.Id:N}";
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = deletedBy;

        var tokens = await _db.Set<RefreshToken>().Where(rt => rt.UserId == userId && !rt.IsRevoked).ToListAsync();
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = "User deleted";
        }

        var sessions = await _db.Set<UserSession>().Where(s => s.UserId == userId && s.IsActive).ToListAsync();
        foreach (var s in sessions)
        {
            s.IsActive = false;
            s.LogoutAt = DateTime.UtcNow;
            s.LogoutReason = "AdminForced";
        }

        await _db.SaveChangesAsync();
        _permissions.InvalidateCache(userId);
    }

    public async Task UnlockUserAsync(Guid userId)
    {
        var user = await _db.Set<ApplicationUser>().FirstAsync(u => u.Id == userId);
        user.IsLockedOut = false;
        user.LockoutEndAt = null;
        user.FailedLoginAttempts = 0;
        await _db.SaveChangesAsync();
    }

    public async Task ForceLogoutUserAsync(Guid userId, string reason)
    {
        var tokens = await _db.Set<RefreshToken>().Where(rt => rt.UserId == userId && !rt.IsRevoked).ToListAsync();
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = reason;
        }

        var sessions = await _db.Set<UserSession>().Where(s => s.UserId == userId && s.IsActive).ToListAsync();
        foreach (var s in sessions)
        {
            s.IsActive = false;
            s.LogoutAt = DateTime.UtcNow;
            s.LogoutReason = reason;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(PaginationRequest request)
    {
        var query = _db.Set<ApplicationUser>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(search) || u.Email.ToLower().Contains(search) || u.FirstName.ToLower().Contains(search) || u.LastName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, request.SortBy))
                : query.OrderBy(e => EF.Property<object>(e, request.SortBy));
        }
        else
        {
            query = query.OrderBy(u => u.FirstName);
        }

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var userDtos = items.Select(u =>
        {
            var roles = u.UserRoles.Where(ur => !ur.Role.IsDeleted).Select(ur => ur.Role.Name).ToList();
            return new UserDto(
                u.Id, u.Username, u.Email, u.PhoneNumber,
                u.FirstName, u.LastName, $"{u.FirstName} {u.LastName}",
                u.EmployeeId, u.IsActive, u.IsTwoFactorEnabled, u.TwoFactorMethod,
                u.FailedLoginAttempts, u.IsLockedOut, u.LastLoginAt,
                u.ProfilePicture, roles, new List<string>(),
                u.CreatedAt, u.UpdatedAt
            );
        }).ToList();

        return new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _db.Set<ApplicationUser>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == id);

        var roles = user.UserRoles.Where(ur => !ur.Role.IsDeleted).Select(ur => ur.Role.Name).ToList();
        var claims = await _permissions.GetUserClaimsAsync(id);

        return new UserDto(
            user.Id, user.Username, user.Email, user.PhoneNumber,
            user.FirstName, user.LastName, $"{user.FirstName} {user.LastName}",
            user.EmployeeId, user.IsActive, user.IsTwoFactorEnabled, user.TwoFactorMethod,
            user.FailedLoginAttempts, user.IsLockedOut, user.LastLoginAt,
            user.ProfilePicture, roles, claims,
            user.CreatedAt, user.UpdatedAt
        );
    }

    public async Task UpdateUserAsync(Guid id, UpdateUserDto dto, string updatedBy)
    {
        var user = await _db.Set<ApplicationUser>().FirstAsync(u => u.Id == id);
        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.IsTwoFactorEnabled.HasValue) user.IsTwoFactorEnabled = dto.IsTwoFactorEnabled.Value;
        if (dto.TwoFactorMethod != null) user.TwoFactorMethod = dto.TwoFactorMethod;
        user.UpdatedBy = updatedBy;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId, string assignedBy)
    {
        var exists = await _db.Set<UserRole>().AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (exists) return;

        _db.Set<UserRole>().Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        _permissions.InvalidateCache(userId);
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await _db.Set<UserRole>().FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole != null)
        {
            _db.Set<UserRole>().Remove(userRole);
            await _db.SaveChangesAsync();
            _permissions.InvalidateCache(userId);
        }
    }

    public async Task AssignUserClaimAsync(Guid userId, Guid claimId, bool isGranted, string assignedBy)
    {
        var existing = await _db.Set<UserClaim>().FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimId == claimId);
        if (existing != null)
        {
            existing.IsGranted = isGranted;
            existing.AssignedBy = assignedBy;
        }
        else
        {
            _db.Set<UserClaim>().Add(new UserClaim
            {
                UserId = userId,
                ClaimId = claimId,
                IsGranted = isGranted,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow,
            });
        }
        await _db.SaveChangesAsync();
        _permissions.InvalidateCache(userId);
    }

    public async Task RemoveUserClaimAsync(Guid userId, Guid claimId)
    {
        var userClaim = await _db.Set<UserClaim>().FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimId == claimId);
        if (userClaim != null)
        {
            _db.Set<UserClaim>().Remove(userClaim);
            await _db.SaveChangesAsync();
            _permissions.InvalidateCache(userId);
        }
    }

    public async Task<List<UserSessionDto>> GetUserSessionsAsync(Guid userId)
    {
        return await _db.Set<UserSession>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginAt)
            .Select(s => new UserSessionDto(
                s.Id, s.UserId, s.DeviceInfo, s.IpAddress, s.Browser, s.Os,
                s.LoginAt, s.LastActivityAt, s.LogoutAt, s.IsActive, s.LogoutReason
            ))
            .ToListAsync();
    }

    public async Task<List<UserAccessLogDto>> GetUserAccessLogsAsync(Guid userId, AccessLogFilterRequest request)
    {
        var query = _db.Set<UserAccessLog>().Where(l => l.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Module))
            query = query.Where(l => l.Module == request.Module);
        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(l => l.Action == request.Action);
        if (request.DateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= request.DateTo.Value);
        if (request.IsSuccess.HasValue)
            query = query.Where(l => l.IsSuccess == request.IsSuccess.Value);

        return await query.OrderByDescending(l => l.CreatedAt)
            .Take(request.PageSize)
            .Select(l => new UserAccessLogDto(
                l.Id, l.UserId, l.Username, l.Action, l.Module, l.EntityType, l.EntityId,
                l.HttpMethod, l.Endpoint, l.ResponseStatusCode, l.IpAddress, l.UserAgent,
                l.DurationMs, l.IsSuccess, l.ErrorMessage, l.OldValues, l.NewValues, l.CreatedAt
            ))
            .ToListAsync();
    }
}
