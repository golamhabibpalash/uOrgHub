using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.ProjectBudgets.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.ProjectBudgets.Queries;

public record GetProjectBudgetsQuery(Guid ProjectId, PaginationRequest Request) : IQuery<PagedResult<ProjectBudgetResponseDto>>;
public record GetProjectBudgetByIdQuery(Guid Id) : IQuery<ProjectBudgetResponseDto>;
public record GetAllProjectBudgetsForExportQuery : IQuery<List<ProjectBudgetResponseDto>>;

public class GetProjectBudgetsQueryHandler : IRequestHandler<GetProjectBudgetsQuery, PagedResult<ProjectBudgetResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectBudgetsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectBudgetResponseDto>> Handle(GetProjectBudgetsQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectBudget>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .AsQueryable();

        query = query.ApplySorting(request.Request.SortBy ?? "BudgetType", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProjectBudgetResponseDto>
        {
            Items = items.Select(BudgetMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectBudgetByIdQueryHandler : IRequestHandler<GetProjectBudgetByIdQuery, ProjectBudgetResponseDto>
{
    private readonly AppDbContext _context;
    public GetProjectBudgetByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectBudgetResponseDto> Handle(GetProjectBudgetByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectBudget>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectBudget), request.Id);

        return BudgetMapper.ToDto(entity);
    }
}

public class GetAllProjectBudgetsForExportQueryHandler : IRequestHandler<GetAllProjectBudgetsForExportQuery, List<ProjectBudgetResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllProjectBudgetsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectBudgetResponseDto>> Handle(GetAllProjectBudgetsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectBudget>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.BudgetType)
            .Select(x => BudgetMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
