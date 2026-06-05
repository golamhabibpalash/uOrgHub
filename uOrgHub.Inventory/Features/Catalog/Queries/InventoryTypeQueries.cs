using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Catalog.Queries;

public record GetInventoryTypesQuery(PaginationRequest Request) : IQuery<PagedResult<InventoryTypeResponseDto>>;
public record GetInventoryTypeByIdQuery(Guid Id) : IQuery<InventoryTypeResponseDto>;

public class GetInventoryTypesQueryHandler : IRequestHandler<GetInventoryTypesQuery, PagedResult<InventoryTypeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetInventoryTypesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<InventoryTypeResponseDto>> Handle(GetInventoryTypesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.InventoryType>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name, x => x.Code);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<InventoryTypeResponseDto>
        {
            Items = items.Select(e => new InventoryTypeResponseDto { Id = e.Id, Name = e.Name, Code = e.Code, Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetInventoryTypeByIdQueryHandler : IRequestHandler<GetInventoryTypeByIdQuery, InventoryTypeResponseDto>
{
    private readonly AppDbContext _context;
    public GetInventoryTypeByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<InventoryTypeResponseDto> Handle(GetInventoryTypeByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.InventoryType>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.InventoryType), request.Id);

        return new InventoryTypeResponseDto { Id = e.Id, Name = e.Name, Code = e.Code, Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt };
    }
}

public record GetAllInventoryTypesForExportQuery : IRequest<List<InventoryTypeResponseDto>>;

public class GetAllInventoryTypesForExportQueryHandler : IRequestHandler<GetAllInventoryTypesForExportQuery, List<InventoryTypeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllInventoryTypesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<InventoryTypeResponseDto>> Handle(GetAllInventoryTypesForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<Models.Entities.InventoryType>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(e => new InventoryTypeResponseDto { Id = e.Id, Name = e.Name, Code = e.Code, Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt })
            .ToListAsync(ct);
    }
}
