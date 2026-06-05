using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.DPR.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.DPR.Queries;

public record GetDPRsQuery(PaginationRequest Request, Guid? ProjectId = null, DPRStatus? Status = null) : IQuery<PagedResult<DPRResponseDto>>;
public record GetDPRByIdQuery(Guid Id) : IQuery<DPRResponseDto>;
public record GetAllDPRsForExportQuery : IQuery<List<DPRResponseDto>>;

public class GetDPRsQueryHandler : IRequestHandler<GetDPRsQuery, PagedResult<DPRResponseDto>>
{
    private readonly AppDbContext _context;
    public GetDPRsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<DPRResponseDto>> Handle(GetDPRsQuery request, CancellationToken ct)
    {
        var query = _context.Set<DailyProgressReport>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.WorkDone);

        query = query.ApplySorting(request.Request.SortBy ?? "ReportDate", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DPRResponseDto>
        {
            Items = items.Select(DPRMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetDPRByIdQueryHandler : IRequestHandler<GetDPRByIdQuery, DPRResponseDto>
{
    private readonly AppDbContext _context;
    public GetDPRByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<DPRResponseDto> Handle(GetDPRByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<DailyProgressReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(DailyProgressReport), request.Id);

        return DPRMapper.ToDto(entity);
    }
}

public class GetAllDPRsForExportQueryHandler : IRequestHandler<GetAllDPRsForExportQuery, List<DPRResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllDPRsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<DPRResponseDto>> Handle(GetAllDPRsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<DailyProgressReport>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ReportDate)
            .Select(x => DPRMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
