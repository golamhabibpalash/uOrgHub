using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.NCRs.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.NCRs.Queries;

public record GetNCRsQuery(PaginationRequest Request, Guid? ProjectId = null, NCRStatus? Status = null, NCRSeverity? Severity = null) : IQuery<PagedResult<NCRResponseDto>>;
public record GetNCRByIdQuery(Guid Id) : IQuery<NCRResponseDto>;
public record GetAllNCRsForExportQuery : IQuery<List<NCRResponseDto>>;

public class GetNCRsQueryHandler : IRequestHandler<GetNCRsQuery, PagedResult<NCRResponseDto>>
{
    private readonly AppDbContext _context;
    public GetNCRsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<NCRResponseDto>> Handle(GetNCRsQuery request, CancellationToken ct)
    {
        var query = _context.Set<NonConformanceReport>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.Severity.HasValue)
            query = query.Where(x => x.Severity == request.Severity.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.NCRNumber);

        query = query.ApplySorting(request.Request.SortBy ?? "NCRNumber", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<NCRResponseDto>
        {
            Items = items.Select(NCRMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetNCRByIdQueryHandler : IRequestHandler<GetNCRByIdQuery, NCRResponseDto>
{
    private readonly AppDbContext _context;
    public GetNCRByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<NCRResponseDto> Handle(GetNCRByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<NonConformanceReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(NonConformanceReport), request.Id);

        return NCRMapper.ToDto(entity);
    }
}

public class GetAllNCRsForExportQueryHandler : IRequestHandler<GetAllNCRsForExportQuery, List<NCRResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllNCRsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<NCRResponseDto>> Handle(GetAllNCRsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<NonConformanceReport>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.NCRNumber)
            .Select(x => NCRMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
