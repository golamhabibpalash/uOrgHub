using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.CoreHR.Queries;

public record GetDesignationsQuery(PaginationRequest Request, Guid? DepartmentId = null) : IQuery<PagedResult<DesignationResponseDto>>;
public record GetDesignationByIdQuery(Guid Id) : IQuery<DesignationResponseDto>;

public class GetDesignationsQueryHandler : IRequestHandler<GetDesignationsQuery, PagedResult<DesignationResponseDto>>
{
    private readonly AppDbContext _context;

    public GetDesignationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<DesignationResponseDto>> Handle(GetDesignationsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Designation>()
            .Include(x => x.Department)
            .Include(x => x.ParentDesignation)
            .Where(x => !x.IsDeleted);

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId);

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

        var dtos = items.Select(e => new DesignationResponseDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            Level = e.Level,
            IsActive = e.IsActive,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            ParentDesignationId = e.ParentDesignationId,
            ParentDesignationName = e.ParentDesignation?.Name,
            SalaryGradeId = e.SalaryGradeId,
            CreatedAt = e.CreatedAt
        }).ToList();

        return new PagedResult<DesignationResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetDesignationByIdQueryHandler : IRequestHandler<GetDesignationByIdQuery, DesignationResponseDto>
{
    private readonly AppDbContext _context;

    public GetDesignationByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<DesignationResponseDto> Handle(GetDesignationByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Designation>()
            .Include(x => x.Department)
            .Include(x => x.ParentDesignation)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Designation), request.Id);

        return new DesignationResponseDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            Level = e.Level,
            IsActive = e.IsActive,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            ParentDesignationId = e.ParentDesignationId,
            ParentDesignationName = e.ParentDesignation?.Name,
            SalaryGradeId = e.SalaryGradeId,
            CreatedAt = e.CreatedAt
        };
    }
}
