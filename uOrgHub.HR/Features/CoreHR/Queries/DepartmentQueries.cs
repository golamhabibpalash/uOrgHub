using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.CoreHR.Queries;

public record GetDepartmentsQuery(PaginationRequest Request) : IQuery<PagedResult<DepartmentResponseDto>>;
public record GetAllDepartmentsQuery : IQuery<List<DepartmentResponseDto>>;
public record GetDepartmentByIdQuery(Guid Id) : IQuery<DepartmentResponseDto>;
public record GetDepartmentDependenciesQuery(Guid Id) : IQuery<DepartmentDependenciesDto>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, PagedResult<DepartmentResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly DepartmentMapper _mapper = new();

    public GetDepartmentsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<DepartmentResponseDto>> Handle(GetDepartmentsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Department>()
            .Include(x => x.HeadOfDepartment)
            .Include(x => x.ParentDepartment)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search) || x.Code.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(e => new DepartmentResponseDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            Description = e.Description,
            Type = e.Type,
            IsActive = e.IsActive,
            ParentDepartmentId = e.ParentDepartmentId,
            ParentDepartmentName = e.ParentDepartment?.Name,
            HeadOfDepartmentId = e.HeadOfDepartmentId,
            HeadOfDepartmentName = e.HeadOfDepartment != null
                ? $"{e.HeadOfDepartment.FirstName} {e.HeadOfDepartment.LastName}"
                : null,
            CreatedAt = e.CreatedAt
        }).ToList();

        return new PagedResult<DepartmentResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentResponseDto>>
{
    private readonly IDepartmentRepository _repo;
    private readonly DepartmentMapper _mapper = new();

    public GetAllDepartmentsQueryHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<List<DepartmentResponseDto>> Handle(GetAllDepartmentsQuery request, CancellationToken ct)
    {
        var entities = await _repo.GetAllForDropdownAsync();
        return entities.Select(e => new DepartmentResponseDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            Description = e.Description,
            Type = e.Type,
            IsActive = e.IsActive,
            ParentDepartmentId = e.ParentDepartmentId,
            ParentDepartmentName = e.ParentDepartment?.Name,
            CreatedAt = e.CreatedAt
        }).ToList();
    }
}

public class GetDepartmentDependenciesQueryHandler : IRequestHandler<GetDepartmentDependenciesQuery, DepartmentDependenciesDto>
{
    private readonly IDepartmentRepository _repo;

    public GetDepartmentDependenciesQueryHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<DepartmentDependenciesDto> Handle(GetDepartmentDependenciesQuery request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Department), request.Id);

        return await _repo.GetDependenciesAsync(request.Id, ct);
    }
}

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentResponseDto>
{
    private readonly AppDbContext _context;

    public GetDepartmentByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<DepartmentResponseDto> Handle(GetDepartmentByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Department>()
            .Include(x => x.HeadOfDepartment)
            .Include(x => x.ParentDepartment)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Department), request.Id);

        return new DepartmentResponseDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            Description = e.Description,
            Type = e.Type,
            IsActive = e.IsActive,
            ParentDepartmentId = e.ParentDepartmentId,
            ParentDepartmentName = e.ParentDepartment?.Name,
            HeadOfDepartmentId = e.HeadOfDepartmentId,
            HeadOfDepartmentName = e.HeadOfDepartment != null
                ? $"{e.HeadOfDepartment.FirstName} {e.HeadOfDepartment.LastName}"
                : null,
            CreatedAt = e.CreatedAt
        };
    }
}
