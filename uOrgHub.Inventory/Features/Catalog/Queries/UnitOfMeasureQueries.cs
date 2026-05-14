using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Catalog.Queries;

public record GetUnitsOfMeasureQuery(PaginationRequest Request) : IQuery<PagedResult<UnitOfMeasureResponseDto>>;
public record GetUnitOfMeasureByIdQuery(Guid Id) : IQuery<UnitOfMeasureResponseDto>;

public class GetUnitsOfMeasureQueryHandler : IRequestHandler<GetUnitsOfMeasureQuery, PagedResult<UnitOfMeasureResponseDto>>
{
    private readonly AppDbContext _context;
    public GetUnitsOfMeasureQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<UnitOfMeasureResponseDto>> Handle(GetUnitsOfMeasureQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.UnitOfMeasure>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search) || x.Abbreviation.Contains(request.Request.Search));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<UnitOfMeasureResponseDto>
        {
            Items = items.Select(e => new UnitOfMeasureResponseDto { Id = e.Id, Name = e.Name, Abbreviation = e.Abbreviation, IsActive = e.IsActive, CreatedAt = e.CreatedAt }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetUnitOfMeasureByIdQueryHandler : IRequestHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureResponseDto>
{
    private readonly AppDbContext _context;
    public GetUnitOfMeasureByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<UnitOfMeasureResponseDto> Handle(GetUnitOfMeasureByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.UnitOfMeasure>()
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.UnitOfMeasure), request.Id);

        return new UnitOfMeasureResponseDto { Id = e.Id, Name = e.Name, Abbreviation = e.Abbreviation, IsActive = e.IsActive, CreatedAt = e.CreatedAt };
    }
}
