using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.BOQ.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.BOQ.Queries;

public record GetBOQsQuery(PaginationRequest Request, Guid? ProjectId = null, BOQStatus? Status = null) : IQuery<PagedResult<BOQResponseDto>>;
public record GetBOQByIdQuery(Guid Id) : IQuery<BOQResponseDto>;
public record GetAllBOQsForExportQuery : IQuery<List<BOQResponseDto>>;

public class GetBOQsQueryHandler : IRequestHandler<GetBOQsQuery, PagedResult<BOQResponseDto>>
{
    private readonly AppDbContext _context;
    public GetBOQsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<BOQResponseDto>> Handle(GetBOQsQuery request, CancellationToken ct)
    {
        var query = _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.BOQNumber);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.BOQNumber);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BOQResponseDto>
        {
            Items = items.Select(BOQMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetBOQByIdQueryHandler : IRequestHandler<GetBOQByIdQuery, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public GetBOQByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(GetBOQByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.Id);

        return BOQMapper.ToDto(entity);
    }
}

public class GetAllBOQsForExportQueryHandler : IRequestHandler<GetAllBOQsForExportQuery, List<BOQResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllBOQsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<BOQResponseDto>> Handle(GetAllBOQsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.BOQNumber)
            .Select(x => BOQMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
