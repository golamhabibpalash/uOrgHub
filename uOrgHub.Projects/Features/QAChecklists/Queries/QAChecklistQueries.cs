using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.QAChecklists.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.QAChecklists.Queries;

public record GetQAChecklistsQuery(PaginationRequest Request, Guid? ProjectId = null, QAChecklistStatus? Status = null) : IQuery<PagedResult<QAChecklistResponseDto>>;
public record GetQAChecklistByIdQuery(Guid Id) : IQuery<QAChecklistResponseDto>;
public record GetAllQAChecklistsForExportQuery : IQuery<List<QAChecklistResponseDto>>;

public class GetQAChecklistsQueryHandler : IRequestHandler<GetQAChecklistsQuery, PagedResult<QAChecklistResponseDto>>
{
    private readonly AppDbContext _context;
    public GetQAChecklistsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<QAChecklistResponseDto>> Handle(GetQAChecklistsQuery request, CancellationToken ct)
    {
        var query = _context.Set<QAChecklist>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.ChecklistNumber);

        query = query.ApplySorting(request.Request.SortBy ?? "ChecklistNumber", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<QAChecklistResponseDto>
        {
            Items = items.Select(QAChecklistMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetQAChecklistByIdQueryHandler : IRequestHandler<GetQAChecklistByIdQuery, QAChecklistResponseDto>
{
    private readonly AppDbContext _context;
    public GetQAChecklistByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<QAChecklistResponseDto> Handle(GetQAChecklistByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<QAChecklist>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(QAChecklist), request.Id);

        return QAChecklistMapper.ToDto(entity);
    }
}

public class GetAllQAChecklistsForExportQueryHandler : IRequestHandler<GetAllQAChecklistsForExportQuery, List<QAChecklistResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllQAChecklistsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<QAChecklistResponseDto>> Handle(GetAllQAChecklistsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<QAChecklist>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ChecklistNumber)
            .Select(x => QAChecklistMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
