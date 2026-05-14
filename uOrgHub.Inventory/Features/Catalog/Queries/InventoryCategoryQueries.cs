using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Catalog.Queries;

public record GetInventoryCategoriesQuery(PaginationRequest Request, Guid? TypeId = null) : IQuery<PagedResult<InventoryCategoryResponseDto>>;
public record GetInventoryCategoryByIdQuery(Guid Id) : IQuery<InventoryCategoryResponseDto>;

public class GetInventoryCategoriesQueryHandler : IRequestHandler<GetInventoryCategoriesQuery, PagedResult<InventoryCategoryResponseDto>>
{
    private readonly AppDbContext _context;
    public GetInventoryCategoriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<InventoryCategoryResponseDto>> Handle(GetInventoryCategoriesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.InventoryCategory>()
            .Include(x => x.Type)
            .Include(x => x.ParentCategory)
            .Where(x => !x.IsDeleted);

        if (request.TypeId.HasValue)
            query = query.Where(x => x.TypeId == request.TypeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search) || x.Code.Contains(request.Request.Search));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<InventoryCategoryResponseDto>
        {
            Items = items.Select(e => new InventoryCategoryResponseDto
            {
                Id = e.Id, Name = e.Name, Code = e.Code,
                TypeId = e.TypeId, TypeName = e.Type?.Name ?? string.Empty,
                ParentCategoryId = e.ParentCategoryId, ParentCategoryName = e.ParentCategory?.Name,
                Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetInventoryCategoryByIdQueryHandler : IRequestHandler<GetInventoryCategoryByIdQuery, InventoryCategoryResponseDto>
{
    private readonly AppDbContext _context;
    public GetInventoryCategoryByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<InventoryCategoryResponseDto> Handle(GetInventoryCategoryByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.InventoryCategory>()
            .Include(x => x.Type)
            .Include(x => x.ParentCategory)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.InventoryCategory), request.Id);

        return new InventoryCategoryResponseDto
        {
            Id = e.Id, Name = e.Name, Code = e.Code,
            TypeId = e.TypeId, TypeName = e.Type?.Name ?? string.Empty,
            ParentCategoryId = e.ParentCategoryId, ParentCategoryName = e.ParentCategory?.Name,
            Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt
        };
    }
}
