using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Items.Queries;

public record GetItemVariantsQuery(PaginationRequest Request, Guid? ItemId = null) : IQuery<PagedResult<ItemVariantResponseDto>>;
public record GetItemVariantByIdQuery(Guid Id) : IQuery<ItemVariantResponseDto>;

public class GetItemVariantsQueryHandler : IRequestHandler<GetItemVariantsQuery, PagedResult<ItemVariantResponseDto>>
{
    private readonly AppDbContext _context;
    public GetItemVariantsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ItemVariantResponseDto>> Handle(GetItemVariantsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.ItemVariant>()
            .Include(x => x.Item)
            .Include(x => x.Attributes).ThenInclude(x => x.AttributeDefinition)
            .Where(x => !x.IsDeleted);

        if (request.ItemId.HasValue) query = query.Where(x => x.ItemId == request.ItemId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.SKU, x => x.VariantName);

        query = query.ApplySorting(request.Request.SortBy ?? "SKU", request.Request.SortDescending);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<ItemVariantResponseDto>
        {
            Items = items.Select(e => MapToDto(e)).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    private static ItemVariantResponseDto MapToDto(Models.Entities.ItemVariant e) => new()
    {
        Id = e.Id, ItemId = e.ItemId, ItemBaseName = e.Item?.BaseName ?? string.Empty,
        SKU = e.SKU, VariantName = e.VariantName, Barcode = e.Barcode,
        CostPrice = e.CostPrice, SellingPrice = e.SellingPrice,
        IsDefault = e.IsDefault, IsActive = e.IsActive, AttributeHash = e.AttributeHash,
        Attributes = e.Attributes.Where(a => !a.IsDeleted).Select(a => new VariantAttributeResponseDto
        {
            Id = a.Id, AttributeDefinitionId = a.AttributeDefinitionId,
            AttributeName = a.AttributeDefinition?.Name ?? string.Empty, Value = a.Value
        }).ToList(),
        CreatedAt = e.CreatedAt
    };
}

public class GetItemVariantByIdQueryHandler : IRequestHandler<GetItemVariantByIdQuery, ItemVariantResponseDto>
{
    private readonly AppDbContext _context;
    public GetItemVariantByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ItemVariantResponseDto> Handle(GetItemVariantByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.ItemVariant>()
            .Include(x => x.Item)
            .Include(x => x.Attributes).ThenInclude(x => x.AttributeDefinition)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.ItemVariant), request.Id);

        return new ItemVariantResponseDto
        {
            Id = e.Id, ItemId = e.ItemId, ItemBaseName = e.Item?.BaseName ?? string.Empty,
            SKU = e.SKU, VariantName = e.VariantName, Barcode = e.Barcode,
            CostPrice = e.CostPrice, SellingPrice = e.SellingPrice,
            IsDefault = e.IsDefault, IsActive = e.IsActive, AttributeHash = e.AttributeHash,
            Attributes = e.Attributes.Where(a => !a.IsDeleted).Select(a => new VariantAttributeResponseDto
            {
                Id = a.Id, AttributeDefinitionId = a.AttributeDefinitionId,
                AttributeName = a.AttributeDefinition?.Name ?? string.Empty, Value = a.Value
            }).ToList(),
            CreatedAt = e.CreatedAt
        };
    }
}

public record GetAllItemVariantsForExportQuery(Guid? ItemId = null) : IRequest<List<ItemVariantResponseDto>>;

public class GetAllItemVariantsForExportQueryHandler : IRequestHandler<GetAllItemVariantsForExportQuery, List<ItemVariantResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllItemVariantsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ItemVariantResponseDto>> Handle(GetAllItemVariantsForExportQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.ItemVariant>()
            .Include(x => x.Item)
            .Include(x => x.Attributes).ThenInclude(x => x.AttributeDefinition)
            .Where(x => !x.IsDeleted);

        if (request.ItemId.HasValue) query = query.Where(x => x.ItemId == request.ItemId.Value);

        var items = await query.OrderBy(x => x.SKU).ToListAsync(ct);

        return items.Select(MapToDto).ToList();
    }

    private static ItemVariantResponseDto MapToDto(Models.Entities.ItemVariant e) => new()
    {
        Id = e.Id, ItemId = e.ItemId, ItemBaseName = e.Item?.BaseName ?? string.Empty,
        SKU = e.SKU, VariantName = e.VariantName, Barcode = e.Barcode,
        CostPrice = e.CostPrice, SellingPrice = e.SellingPrice,
        IsDefault = e.IsDefault, IsActive = e.IsActive, AttributeHash = e.AttributeHash,
        Attributes = e.Attributes.Where(a => !a.IsDeleted).Select(a => new VariantAttributeResponseDto
        {
            Id = a.Id, AttributeDefinitionId = a.AttributeDefinitionId,
            AttributeName = a.AttributeDefinition?.Name ?? string.Empty, Value = a.Value
        }).ToList(),
        CreatedAt = e.CreatedAt
    };
}
