using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.WBS.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.WBS.Queries;

public record GetWBSItemsQuery(Guid ProjectId, PaginationRequest Request) : IQuery<PagedResult<WBSResponseDto>>;
public record GetWBSByIdQuery(Guid Id) : IQuery<WBSResponseDto>;
public record GetAllWBSForExportQuery : IQuery<List<WBSResponseDto>>;

public class GetWBSItemsQueryHandler : IRequestHandler<GetWBSItemsQuery, PagedResult<WBSResponseDto>>
{
    private readonly AppDbContext _context;
    public GetWBSItemsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<WBSResponseDto>> Handle(GetWBSItemsQuery request, CancellationToken ct)
    {
        var query = _context.Set<WorkBreakdownStructure>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.WBSCode);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Sequence)
            : query.OrderBy(x => x.Sequence);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<WBSResponseDto>
        {
            Items = items.Select(WBSMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetWBSByIdQueryHandler : IRequestHandler<GetWBSByIdQuery, WBSResponseDto>
{
    private readonly AppDbContext _context;
    public GetWBSByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<WBSResponseDto> Handle(GetWBSByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<WorkBreakdownStructure>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkBreakdownStructure), request.Id);

        return WBSMapper.ToDto(entity);
    }
}

public class GetAllWBSForExportQueryHandler : IRequestHandler<GetAllWBSForExportQuery, List<WBSResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllWBSForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<WBSResponseDto>> Handle(GetAllWBSForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<WorkBreakdownStructure>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.WBSCode)
            .Select(x => WBSMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
