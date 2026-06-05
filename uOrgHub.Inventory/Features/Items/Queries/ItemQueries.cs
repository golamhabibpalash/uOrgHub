using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Items.Queries;

public record GetItemsQuery(PaginationRequest Request, Guid? CategoryId = null, Guid? TypeId = null) : IQuery<PagedResult<ItemResponseDto>>;
public record GetAllItemsQuery(Guid? CategoryId = null, Guid? TypeId = null, string? Search = null) : IQuery<List<ItemResponseDto>>;
public record GetItemByIdQuery(Guid Id) : IQuery<ItemResponseDto>;

public class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, PagedResult<ItemResponseDto>>
{
    private readonly AppDbContext _context;
    public GetItemsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ItemResponseDto>> Handle(GetItemsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Item>()
            .Include(x => x.Type)
            .Include(x => x.Category)
            .Include(x => x.UnitOfMeasure)
            .Where(x => !x.IsDeleted);

        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.TypeId.HasValue) query = query.Where(x => x.TypeId == request.TypeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.BaseName.Contains(request.Request.Search) || (x.ItemCode != null && x.ItemCode.Contains(request.Request.Search)));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.BaseName) : query.OrderBy(x => x.BaseName);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        var itemIds = items.Select(i => i.Id).ToList();
        var variantCounts = await _context.Set<Models.Entities.ItemVariant>()
            .Where(v => itemIds.Contains(v.ItemId) && !v.IsDeleted)
            .GroupBy(v => v.ItemId)
            .Select(g => new { ItemId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ItemId, x => x.Count, ct);

        return new PagedResult<ItemResponseDto>
        {
            Items = items.Select(e => new ItemResponseDto
            {
                Id = e.Id, BaseName = e.BaseName, ItemCode = e.ItemCode,
                TypeId = e.TypeId, TypeName = e.Type?.Name ?? string.Empty,
                CategoryId = e.CategoryId, CategoryName = e.Category?.Name ?? string.Empty,
                UnitOfMeasureId = e.UnitOfMeasureId, UnitOfMeasureName = e.UnitOfMeasure?.Name ?? string.Empty,
                UnitAbbreviation = e.UnitOfMeasure?.Abbreviation ?? string.Empty,
                Brand = e.Brand, Manufacturer = e.Manufacturer, Description = e.Description,
                ReorderLevel = e.ReorderLevel, StandardCost = e.StandardCost,
                IsActive = e.IsActive, VariantCount = variantCounts.GetValueOrDefault(e.Id, 0), CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, List<ItemResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllItemsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ItemResponseDto>> Handle(GetAllItemsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Item>()
            .Include(x => x.Type).Include(x => x.Category).Include(x => x.UnitOfMeasure)
            .Where(x => !x.IsDeleted);

        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.TypeId.HasValue) query = query.Where(x => x.TypeId == request.TypeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.BaseName.Contains(request.Search) || (x.ItemCode != null && x.ItemCode.Contains(request.Search)));

        query = query.OrderBy(x => x.BaseName);
        var items = await query.ToListAsync(ct);

        var itemIds = items.Select(i => i.Id).ToList();
        var variantCounts = await _context.Set<Models.Entities.ItemVariant>()
            .Where(v => itemIds.Contains(v.ItemId) && !v.IsDeleted)
            .GroupBy(v => v.ItemId)
            .Select(g => new { ItemId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ItemId, x => x.Count, ct);

        return items.Select(e => new ItemResponseDto
        {
            Id = e.Id, BaseName = e.BaseName, ItemCode = e.ItemCode,
            TypeId = e.TypeId, TypeName = e.Type?.Name ?? string.Empty,
            CategoryId = e.CategoryId, CategoryName = e.Category?.Name ?? string.Empty,
            UnitOfMeasureId = e.UnitOfMeasureId, UnitOfMeasureName = e.UnitOfMeasure?.Name ?? string.Empty,
            UnitAbbreviation = e.UnitOfMeasure?.Abbreviation ?? string.Empty,
            Brand = e.Brand, Manufacturer = e.Manufacturer, Description = e.Description,
            ReorderLevel = e.ReorderLevel, StandardCost = e.StandardCost,
            IsActive = e.IsActive, VariantCount = variantCounts.GetValueOrDefault(e.Id, 0), CreatedAt = e.CreatedAt
        }).ToList();
    }
}

public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemResponseDto>
{
    private readonly AppDbContext _context;
    public GetItemByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ItemResponseDto> Handle(GetItemByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Item>()
            .Include(x => x.Type).Include(x => x.Category).Include(x => x.UnitOfMeasure)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Item), request.Id);

        var variantCount = await _context.Set<Models.Entities.ItemVariant>().CountAsync(x => x.ItemId == e.Id && !x.IsDeleted, ct);

        return new ItemResponseDto
        {
            Id = e.Id, BaseName = e.BaseName, ItemCode = e.ItemCode,
            TypeId = e.TypeId, TypeName = e.Type?.Name ?? string.Empty,
            CategoryId = e.CategoryId, CategoryName = e.Category?.Name ?? string.Empty,
            UnitOfMeasureId = e.UnitOfMeasureId, UnitOfMeasureName = e.UnitOfMeasure?.Name ?? string.Empty,
            UnitAbbreviation = e.UnitOfMeasure?.Abbreviation ?? string.Empty,
            Brand = e.Brand, Manufacturer = e.Manufacturer, Description = e.Description,
            ReorderLevel = e.ReorderLevel, StandardCost = e.StandardCost,
            IsActive = e.IsActive, VariantCount = variantCount, CreatedAt = e.CreatedAt
        };
    }
}
