using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.RABills.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.RABills.Queries;

public record GetRABillsQuery(PaginationRequest Request, Guid? ProjectId = null, RABillStatus? Status = null) : IQuery<PagedResult<RABillResponseDto>>;
public record GetRABillByIdQuery(Guid Id) : IQuery<RABillResponseDto>;
public record GetAllRABillsForExportQuery : IQuery<List<RABillResponseDto>>;

public class GetRABillsQueryHandler : IRequestHandler<GetRABillsQuery, PagedResult<RABillResponseDto>>
{
    private readonly AppDbContext _context;
    public GetRABillsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<RABillResponseDto>> Handle(GetRABillsQuery request, CancellationToken ct)
    {
        var query = _context.Set<RABill>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.BillNumber);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.BillDate)
            : query.OrderBy(x => x.BillSequence);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<RABillResponseDto>
        {
            Items = items.Select(RABillMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetRABillByIdQueryHandler : IRequestHandler<GetRABillByIdQuery, RABillResponseDto>
{
    private readonly AppDbContext _context;
    public GetRABillByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<RABillResponseDto> Handle(GetRABillByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<RABill>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RABill), request.Id);

        return RABillMapper.ToDto(entity);
    }
}

public class GetAllRABillsForExportQueryHandler : IRequestHandler<GetAllRABillsForExportQuery, List<RABillResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllRABillsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<RABillResponseDto>> Handle(GetAllRABillsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<RABill>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.BillNumber)
            .Select(x => RABillMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
