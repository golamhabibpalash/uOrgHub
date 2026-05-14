using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.ProjectCategories.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.ProjectCategories.Queries;

public record GetProjectCategoriesQuery(PaginationRequest Request) : IQuery<PagedResult<ProjectCategoryResponseDto>>;
public record GetProjectCategoryByIdQuery(Guid Id) : IQuery<ProjectCategoryResponseDto>;

public class GetProjectCategoriesQueryHandler : IRequestHandler<GetProjectCategoriesQuery, PagedResult<ProjectCategoryResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectCategoriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectCategoryResponseDto>> Handle(GetProjectCategoriesQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectCategory>().Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search) || x.Code.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

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
