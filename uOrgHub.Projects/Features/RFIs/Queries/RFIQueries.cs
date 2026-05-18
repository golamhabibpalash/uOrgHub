using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.RFIs.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.RFIs.Queries;

public record GetRFIsQuery(PaginationRequest Request, Guid? ProjectId = null, RFIStatus? Status = null) : IQuery<PagedResult<RFIResponseDto>>;
public record GetRFIByIdQuery(Guid Id) : IQuery<RFIResponseDto>;

public class GetRFIsQueryHandler : IRequestHandler<GetRFIsQuery, PagedResult<RFIResponseDto>>
{
    private readonly AppDbContext _context;
    public GetRFIsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<RFIResponseDto>> Handle(GetRFIsQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectRFI>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Subject.Contains(request.Request.Search) || x.RFINumber.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.RaisedDate)
            : query.OrderBy(x => x.RFINumber);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<RFIResponseDto>
        {
            Items = items.Select(RFIMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetRFIByIdQueryHandler : IRequestHandler<GetRFIByIdQuery, RFIResponseDto>
{
    private readonly AppDbContext _context;
    public GetRFIByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<RFIResponseDto> Handle(GetRFIByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectRFI>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectRFI), request.Id);

        return RFIMapper.ToDto(entity);
    }
}
