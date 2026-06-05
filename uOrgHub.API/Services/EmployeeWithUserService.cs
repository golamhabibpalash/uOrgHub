using Microsoft.EntityFrameworkCore;
using uOrgHub.API.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.API.Services;

public class EmployeeWithUserService : IEmployeeWithUserService
{
    private readonly AppDbContext _db;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly EmployeeMapper _mapper = new();

    public EmployeeWithUserService(AppDbContext db, IEmployeeRepository employeeRepo)
    {
        _db = db;
        _employeeRepo = employeeRepo;
    }

    public async Task<EmployeeResponseDto> CreateEmployeeWithUserAsync(CreateEmployeeWithUserDto dto)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            if (string.IsNullOrWhiteSpace(dto.Employee.EmployeeCode))
                dto.Employee.EmployeeCode = await _employeeRepo.GetNextEmployeeCodeAsync();
            else if (await _employeeRepo.CodeExistsAsync(dto.Employee.EmployeeCode))
                throw new AppException($"Employee code '{dto.Employee.EmployeeCode}' already exists.");

            if (await _employeeRepo.EmailExistsAsync(dto.Employee.Email))
                throw new AppException($"Email '{dto.Employee.Email}' already in use.");

            var employee = _mapper.ToEntity(dto.Employee);
            employee.CreatedAt = DateTime.UtcNow;
            _db.Set<HR.Models.Entities.Employee>().Add(employee);
            await _db.SaveChangesAsync();

            if (dto.CreateUserAccount && dto.UserAccount != null)
            {
                var userEmail = !string.IsNullOrWhiteSpace(dto.UserAccount.Email)
                    ? dto.UserAccount.Email
                    : dto.Employee.Email;

                if (await _db.Set<ApplicationUser>().AnyAsync(u => u.Username == dto.UserAccount.Username && !u.IsDeleted))
                    throw new AppException($"Username '{dto.UserAccount.Username}' is already taken.");

                if (await _db.Set<ApplicationUser>().AnyAsync(u => u.Email == userEmail && !u.IsDeleted))
                    throw new AppException($"Email '{userEmail}' is already in use by another user.");

                var password = dto.UserAccount.AutoGeneratePassword
                    ? GenerateRandomPassword()
                    : dto.UserAccount.Password;

                var user = new ApplicationUser
                {
                    Username = dto.UserAccount.Username,
                    Email = userEmail,
                    FirstName = dto.Employee.FirstName,
                    LastName = dto.Employee.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
                    EmployeeId = employee.Id,
                    IsActive = dto.UserAccount.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    MustChangePassword = true,
                };

                _db.Set<ApplicationUser>().Add(user);
                await _db.SaveChangesAsync();

                if (dto.UserAccount.RoleIds.Count > 0)
                {
                    foreach (var roleId in dto.UserAccount.RoleIds)
                    {
                        _db.Set<UserRole>().Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roleId,
                            AssignedAt = DateTime.UtcNow,
                        });
                    }
                    await _db.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();

            await _db.Entry(employee).Reference(x => x.Department).LoadAsync();
            await _db.Entry(employee).Reference(x => x.Designation).LoadAsync();

            var result = _mapper.ToDto(employee);
            result.ProfilePictureUrl = EmployeePictureUrl.ToPublicUrl(employee.ProfilePicturePath);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string GenerateRandomPassword()
    {
        var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var lower = "abcdefghijklmnopqrstuvwxyz";
        var digits = "0123456789";
        var all = upper + lower + digits;
        var rng = Random.Shared;
        var chars = new char[12];

        chars[0] = upper[rng.Next(upper.Length)];
        chars[1] = digits[rng.Next(digits.Length)];
        for (int i = 2; i < 12; i++)
            chars[i] = all[rng.Next(all.Length)];

        return new string(chars);
    }
}
