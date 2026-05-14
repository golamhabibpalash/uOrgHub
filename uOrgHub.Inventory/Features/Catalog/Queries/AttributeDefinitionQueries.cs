using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Catalog.Queries;

public record GetAttributeDefinitionsQuery(PaginationRequest Request, Guid? CategoryId = null) : IQuery<PagedResult<AttributeDefinitionResponseDto>>;
public record GetAttributeDefinitionByIdQuery(Guid Id) : IQuery<AttributeDefinitionResponseDto>;

public class GetAttributeDefinitionsQueryHandler : IRequestHandler<GetAttributeDefinitionsQuery, PagedResult<AttributeDefinitionResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAttributeDefinitionsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<AttributeDefinitionResponseDto>> Handle(GetAttributeDefinitionsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.AttributeDefinition>()
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted);

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<AttributeDefinitionResponseDto>
        {
            Items = items.Select(e => new AttributeDefinitionResponseDto
            {
                Id = e.Id, Name = e.Name, DataType = e.DataType,
                CategoryId = e.CategoryId, CategoryName = e.Category?.Name,
                IsRequired = e.IsRequired, PredefinedValues = e.PredefinedValues,
                IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAttributeDefinitionByIdQueryHandler : IRequestHandler<GetAttributeDefinitionByIdQuery, AttributeDefinitionResponseDto>
{
    private readonly AppDbContext _context;
    public GetAttributeDefinitionByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<AttributeDefinitionResponseDto> Handle(GetAttributeDefinitionByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.AttributeDefinition>()
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AttributeDefinition), request.Id);

        return new AttributeDefinitionResponseDto
        {
            Id = e.Id, Name = e.Name, DataType = e.DataType,
            CategoryId = e.CategoryId, CategoryName = e.Category?.Name,
            IsRequired = e.IsRequired, PredefinedValues = e.PredefinedValues,
            IsActive = e.IsActive, CreatedAt = e.CreatedAt
        };
    }
}
