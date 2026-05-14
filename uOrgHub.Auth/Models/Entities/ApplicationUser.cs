using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Auth.Models.Entities;

[Table("auth_users")]
public class ApplicationUser : BaseEntity
{
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsTwoFactorEnabled { get; set; } = false;

    [MaxLength(20)]
    public string TwoFactorMethod { get; set; } = "None";

    public DateTime? LastLoginAt { get; set; }

    [MaxLength(50)]
    public string? LastLoginIp { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockoutEndAt { get; set; }

    public bool IsLockedOut { get; set; } = false;

    public DateTime? PasswordChangedAt { get; set; }

    public bool MustChangePassword { get; set; } = false;

    [MaxLength(500)]
    public string? ProfilePicture { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<TwoFactorOTP> TwoFactorOTPs { get; set; } = new List<TwoFactorOTP>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<UserAccessLog> AccessLogs { get; set; } = new List<UserAccessLog>();
}
