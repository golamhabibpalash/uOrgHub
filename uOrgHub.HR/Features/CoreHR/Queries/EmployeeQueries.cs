using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.CoreHR.Queries;

public record GetEmployeesQuery(PaginationRequest Request, Guid? DepartmentId = null, Guid? DesignationId = null) : IQuery<PagedResult<EmployeeResponseDto>>;
public record GetAllEmployeesQuery(Guid? DepartmentId = null, Guid? DesignationId = null, string? Search = null) : IQuery<List<EmployeeResponseDto>>;
public record GetEmployeeByIdQuery(Guid Id) : IQuery<EmployeeResponseDto>;
public record GetEmployeeDependenciesQuery(Guid Id) : IQuery<EmployeeDependenciesDto>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeResponseDto>>
{
    private readonly AppDbContext _context;

    public GetEmployeesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<EmployeeResponseDto>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Employee>()
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Manager)
            .Where(x => !x.IsDeleted);

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId);

        if (request.DesignationId.HasValue)
            query = query.Where(x => x.DesignationId == request.DesignationId);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x =>
                x.FirstName.Contains(request.Request.Search) ||
                x.LastName.Contains(request.Request.Search) ||
                x.EmployeeCode.Contains(request.Request.Search) ||
                x.Email.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.EmployeeCode)
            : query.OrderBy(x => x.EmployeeCode);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<EmployeeResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }

    private static EmployeeResponseDto MapToDto(Models.Entities.Employee e) => new()
    {
        Id = e.Id,
        EmployeeCode = e.EmployeeCode,
        FirstName = e.FirstName,
        MiddleName = e.MiddleName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        Gender = e.Gender,
        Religion = e.Religion,
        MaritalStatus = e.MaritalStatus,
        BloodGroup = e.BloodGroup,
        DateOfBirth = e.DateOfBirth,
        NationalId = e.NationalId,
        PassportNo = e.PassportNo,
        Nationality = e.Nationality,
        PermanentAddress = e.PermanentAddress,
        CurrentAddress = e.CurrentAddress,
        District = e.District,
        Division = e.Division,
        JoiningDate = e.JoiningDate,
        ConfirmationDate = e.ConfirmationDate,
        EmploymentType = e.EmploymentType,
        Status = e.Status,
        DesignationId = e.DesignationId,
        DesignationName = e.Designation?.Name ?? string.Empty,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name ?? string.Empty,
        ManagerId = e.ManagerId,
        ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
        SalaryGradeId = e.SalaryGradeId,
        BasicSalary = e.BasicSalary,
        CreatedAt = e.CreatedAt
    };
}

public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, List<EmployeeResponseDto>>
{
    private readonly AppDbContext _context;

    public GetAllEmployeesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<EmployeeResponseDto>> Handle(GetAllEmployeesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Employee>()
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Manager)
            .Where(x => !x.IsDeleted);

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId);

        if (request.DesignationId.HasValue)
            query = query.Where(x => x.DesignationId == request.DesignationId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x =>
                x.FirstName.Contains(request.Search) ||
                x.LastName.Contains(request.Search) ||
                x.EmployeeCode.Contains(request.Search) ||
                x.Email.Contains(request.Search));

        query = query.OrderBy(x => x.EmployeeCode);

        var items = await query.ToListAsync(ct);

        return items.Select(e => new EmployeeResponseDto
        {
            Id = e.Id, EmployeeCode = e.EmployeeCode,
            FirstName = e.FirstName, MiddleName = e.MiddleName, LastName = e.LastName,
            Email = e.Email, Phone = e.Phone,
            Gender = e.Gender, Religion = e.Religion, MaritalStatus = e.MaritalStatus,
            BloodGroup = e.BloodGroup, DateOfBirth = e.DateOfBirth,
            NationalId = e.NationalId, PassportNo = e.PassportNo, Nationality = e.Nationality,
            PermanentAddress = e.PermanentAddress, CurrentAddress = e.CurrentAddress,
            District = e.District, Division = e.Division,
            JoiningDate = e.JoiningDate, ConfirmationDate = e.ConfirmationDate,
            EmploymentType = e.EmploymentType, Status = e.Status,
            DesignationId = e.DesignationId, DesignationName = e.Designation?.Name ?? string.Empty,
            DepartmentId = e.DepartmentId, DepartmentName = e.Department?.Name ?? string.Empty,
            ManagerId = e.ManagerId, ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
            SalaryGradeId = e.SalaryGradeId, BasicSalary = e.BasicSalary,
            CreatedAt = e.CreatedAt
        }).ToList();
    }
}

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponseDto>
{
    private readonly AppDbContext _context;

    public GetEmployeeByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeResponseDto> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Employee>()
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Manager)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Employee), request.Id);

        return new EmployeeResponseDto
        {
            Id = e.Id,
            EmployeeCode = e.EmployeeCode,
            FirstName = e.FirstName,
            MiddleName = e.MiddleName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Gender = e.Gender,
            Religion = e.Religion,
            MaritalStatus = e.MaritalStatus,
            BloodGroup = e.BloodGroup,
            DateOfBirth = e.DateOfBirth,
            NationalId = e.NationalId,
            PassportNo = e.PassportNo,
            Nationality = e.Nationality,
            PermanentAddress = e.PermanentAddress,
            CurrentAddress = e.CurrentAddress,
            District = e.District,
            Division = e.Division,
            JoiningDate = e.JoiningDate,
            ConfirmationDate = e.ConfirmationDate,
            EmploymentType = e.EmploymentType,
            Status = e.Status,
            DesignationId = e.DesignationId,
            DesignationName = e.Designation?.Name ?? string.Empty,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            ManagerId = e.ManagerId,
            ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
            SalaryGradeId = e.SalaryGradeId,
            BasicSalary = e.BasicSalary,
            CreatedAt = e.CreatedAt
        };
    }
}

public class GetEmployeeDependenciesQueryHandler : IRequestHandler<GetEmployeeDependenciesQuery, EmployeeDependenciesDto>
{
    private readonly IEmployeeRepository _repo;

    public GetEmployeeDependenciesQueryHandler(IEmployeeRepository repo) => _repo = repo;

    public async Task<EmployeeDependenciesDto> Handle(GetEmployeeDependenciesQuery request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Employee), request.Id);

        return await _repo.GetDependenciesAsync(request.Id, ct);
    }
}
