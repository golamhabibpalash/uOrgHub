using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.ProjectMilestones.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.ProjectMilestones.Queries;

public record GetProjectMilestonesListQuery(Guid ProjectId, PaginationRequest Request, MilestoneStatus? Status = null) : IQuery<PagedResult<ProjectMilestoneResponseDto>>;
public record GetProjectMilestoneByIdQuery(Guid Id) : IQuery<ProjectMilestoneResponseDto>;
public record GetAllProjectMilestonesForExportQuery : IQuery<List<ProjectMilestoneResponseDto>>;

public class GetProjectMilestonesListQueryHandler : IRequestHandler<GetProjectMilestonesListQuery, PagedResult<ProjectMilestoneResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectMilestonesListQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectMilestoneResponseDto>> Handle(GetProjectMilestonesListQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectMilestone>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.PlannedDate)
            : query.OrderBy(x => x.PlannedDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProjectMilestoneResponseDto>
        {
            Items = items.Select(MilestoneMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectMilestoneByIdQueryHandler : IRequestHandler<GetProjectMilestoneByIdQuery, ProjectMilestoneResponseDto>
{
    private readonly AppDbContext _context;
    public GetProjectMilestoneByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectMilestoneResponseDto> Handle(GetProjectMilestoneByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMilestone>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMilestone), request.Id);

        return MilestoneMapper.ToDto(entity);
    }
}

public class GetAllProjectMilestonesForExportQueryHandler : IRequestHandler<GetAllProjectMilestonesForExportQuery, List<ProjectMilestoneResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllProjectMilestonesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectMilestoneResponseDto>> Handle(GetAllProjectMilestonesForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectMilestone>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Title)
            .Select(x => MilestoneMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
