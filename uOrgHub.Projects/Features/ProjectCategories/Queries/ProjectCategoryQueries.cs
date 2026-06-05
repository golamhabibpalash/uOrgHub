using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.ProjectCategories.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.ProjectCategories.Queries;

public record GetProjectCategoriesQuery(PaginationRequest Request) : IQuery<PagedResult<ProjectCategoryResponseDto>>;
public record GetProjectCategoryByIdQuery(Guid Id) : IQuery<ProjectCategoryResponseDto>;
public record GetAllProjectCategoriesForExportQuery : IQuery<List<ProjectCategoryResponseDto>>;

public class GetProjectCategoriesQueryHandler : IRequestHandler<GetProjectCategoriesQuery, PagedResult<ProjectCategoryResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectCategoriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectCategoryResponseDto>> Handle(GetProjectCategoriesQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectCategory>().Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name, x => x.Code);

        query = query.ApplySorting(request.Request.SortBy ?? "Name", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProjectCategoryResponseDto>
        {
            Items = items.Select(ProjectCategoryMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectCategoryByIdQueryHandler : IRequestHandler<GetProjectCategoryByIdQuery, ProjectCategoryResponseDto>
{
    private readonly AppDbContext _context;
    public GetProjectCategoryByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectCategoryResponseDto> Handle(GetProjectCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectCategory>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectCategory), request.Id);

        return ProjectCategoryMapper.ToDto(entity);
    }
}

public class GetAllProjectCategoriesForExportQueryHandler : IRequestHandler<GetAllProjectCategoriesForExportQuery, List<ProjectCategoryResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllProjectCategoriesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectCategoryResponseDto>> Handle(GetAllProjectCategoriesForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectCategory>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => ProjectCategoryMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
