using uOrgHub.Auth.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Auth.Reporting.ExportColumns;

public static class UserExportColumns
{
    public static List<ExportColumn<UserDto>> Get() =>
    [
        new("username", "Username", x => x.Username),
        new("firstName", "First Name", x => x.FirstName),
        new("lastName", "Last Name", x => x.LastName),
        new("fullName", "Full Name", x => x.FullName),
        new("email", "Email", x => x.Email),
        new("phoneNumber", "Phone Number", x => x.PhoneNumber),
        new("isActive", "Is Active", x => x.IsActive),
        new("isLockedOut", "Is Locked Out", x => x.IsLockedOut),
        new("isTwoFactorEnabled", "2FA Enabled", x => x.IsTwoFactorEnabled),
        new("failedLoginAttempts", "Failed Login Attempts", x => x.FailedLoginAttempts),
        new("lastLoginAt", "Last Login", x => x.LastLoginAt),
        new("roles", "Roles", x => string.Join(", ", x.Roles)),
        new("createdAt", "Created At", x => x.CreatedAt),
        new("updatedAt", "Updated At", x => x.UpdatedAt),
    ];
}
