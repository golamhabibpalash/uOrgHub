using Microsoft.AspNetCore.Http;
using uOrgHub.API.Services.Storage;
using uOrgHub.Auth.Repositories;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.API.Services;

public class EmployeeProfilePictureService
{
    private readonly IEmployeeRepository _employees;
    private readonly IUserRepository _users;
    private readonly IFileStorageService _storage;

    public EmployeeProfilePictureService(
        IEmployeeRepository employees,
        IUserRepository users,
        IFileStorageService storage)
    {
        _employees = employees;
        _users = users;
        _storage = storage;
    }

    public async Task<string> UploadForEmployeeAsync(Guid employeeId, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new AppException("No file provided.", 400);

        var employee = await _employees.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        var result = await _storage.SaveAsync(file, $"employees/{employeeId}", employee.ProfilePicturePath, ct);

        employee.ProfilePicturePath = result.RelativePath;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employees.UpdateAsync(employee);

        await SyncToLinkedUserAsync(employeeId, result.RelativePath);

        return result.PublicUrl;
    }

    public async Task<string> UploadForCurrentUserAsync(Guid userId, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new AppException("No file provided.", 400);

        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("ApplicationUser", userId);

        if (!user.EmployeeId.HasValue)
            throw new AppException("Your user account is not linked to an employee record. Contact HR to upload a profile picture.", 400);

        var employee = await _employees.GetByIdAsync(user.EmployeeId.Value)
            ?? throw new NotFoundException(nameof(Employee), user.EmployeeId.Value);

        var result = await _storage.SaveAsync(file, $"employees/{employee.Id}", employee.ProfilePicturePath, ct);

        employee.ProfilePicturePath = result.RelativePath;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employees.UpdateAsync(employee);

        user.ProfilePicture = result.RelativePath;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        return result.PublicUrl;
    }

    public async Task DeleteForEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var employee = await _employees.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        var old = employee.ProfilePicturePath;
        employee.ProfilePicturePath = null;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employees.UpdateAsync(employee);

        await _storage.DeleteAsync(old);
        await ClearLinkedUserPictureAsync(employeeId);
    }

    public async Task DeleteForCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new NotFoundException("ApplicationUser", userId);

        if (!user.EmployeeId.HasValue)
        {
            user.ProfilePicture = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);
            return;
        }

        await DeleteForEmployeeAsync(user.EmployeeId.Value, ct);
    }

    private async Task SyncToLinkedUserAsync(Guid employeeId, string relativePath)
    {
        var user = await _users.GetByEmployeeIdAsync(employeeId);
        if (user is null) return;
        user.ProfilePicture = relativePath;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
    }

    private async Task ClearLinkedUserPictureAsync(Guid employeeId)
    {
        var user = await _users.GetByEmployeeIdAsync(employeeId);
        if (user is null) return;
        user.ProfilePicture = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
    }
}
