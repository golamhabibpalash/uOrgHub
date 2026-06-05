using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Warehouse.Queries;

public record GetWarehousesQuery(PaginationRequest Request) : IQuery<PagedResult<WarehouseResponseDto>>;
public record GetWarehouseByIdQuery(Guid Id) : IQuery<WarehouseResponseDto>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, PagedResult<WarehouseResponseDto>>
{
    private readonly AppDbContext _context;
    public GetWarehousesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<WarehouseResponseDto>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Warehouse>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name, x => x.Code);

        query = query.ApplySorting(request.Request.SortBy ?? "Name", request.Request.SortDescending);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<WarehouseResponseDto>
        {
            Items = items.Select(e => new WarehouseResponseDto
            {
                Id = e.Id, Name = e.Name, Code = e.Code,
                Location = e.Location, ContactPerson = e.ContactPerson, ContactPhone = e.ContactPhone,
                IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseResponseDto>
{
    private readonly AppDbContext _context;
    public GetWarehouseByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<WarehouseResponseDto> Handle(GetWarehouseByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Warehouse>()
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Warehouse), request.Id);

        return new WarehouseResponseDto
        {
            Id = e.Id, Name = e.Name, Code = e.Code,
            Location = e.Location, ContactPerson = e.ContactPerson, ContactPhone = e.ContactPhone,
            IsActive = e.IsActive, CreatedAt = e.CreatedAt
        };
    }
}

public record GetAllWarehousesForExportQuery : IRequest<List<WarehouseResponseDto>>;

public class GetAllWarehousesForExportQueryHandler : IRequestHandler<GetAllWarehousesForExportQuery, List<WarehouseResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllWarehousesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<WarehouseResponseDto>> Handle(GetAllWarehousesForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<Models.Entities.Warehouse>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(e => new WarehouseResponseDto
            {
                Id = e.Id, Name = e.Name, Code = e.Code,
                Location = e.Location, ContactPerson = e.ContactPerson, ContactPhone = e.ContactPhone,
                IsActive = e.IsActive, CreatedAt = e.CreatedAt
            })
            .ToListAsync(ct);
    }
}
