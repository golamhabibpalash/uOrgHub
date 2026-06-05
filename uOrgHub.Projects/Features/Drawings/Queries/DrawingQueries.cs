using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.Drawings.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.Drawings.Queries;

public record GetDrawingsQuery(PaginationRequest Request, Guid? ProjectId = null, DrawingStatus? Status = null, DrawingDiscipline? Discipline = null) : IQuery<PagedResult<DrawingResponseDto>>;
public record GetDrawingByIdQuery(Guid Id) : IQuery<DrawingResponseDto>;
public record GetAllDrawingsForExportQuery : IQuery<List<DrawingResponseDto>>;

public class GetDrawingsQueryHandler : IRequestHandler<GetDrawingsQuery, PagedResult<DrawingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetDrawingsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<DrawingResponseDto>> Handle(GetDrawingsQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectDrawing>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.Discipline.HasValue)
            query = query.Where(x => x.Discipline == request.Discipline.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.DrawingNumber);

        query = query.ApplySorting(request.Request.SortBy ?? "DrawingNumber", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DrawingResponseDto>
        {
            Items = items.Select(DrawingMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetDrawingByIdQueryHandler : IRequestHandler<GetDrawingByIdQuery, DrawingResponseDto>
{
    private readonly AppDbContext _context;
    public GetDrawingByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<DrawingResponseDto> Handle(GetDrawingByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectDrawing>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectDrawing), request.Id);

        return DrawingMapper.ToDto(entity);
    }
}

public class GetAllDrawingsForExportQueryHandler : IRequestHandler<GetAllDrawingsForExportQuery, List<DrawingResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllDrawingsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<DrawingResponseDto>> Handle(GetAllDrawingsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectDrawing>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DrawingNumber)
            .Select(x => DrawingMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
