using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Items.Commands;

public record CreateItemCommand(CreateItemDto Dto) : ICommand<ItemResponseDto>;
public record UpdateItemCommand(Guid Id, UpdateItemDto Dto) : ICommand<ItemResponseDto>;
public record DeleteItemCommand(Guid Id) : ICommand<Unit>;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemResponseDto>
{
    private readonly IItemRepository _repo;
    private readonly AppDbContext _context;

    public CreateItemCommandHandler(IItemRepository repo, AppDbContext context) { _repo = repo; _context = context; }

    public async Task<ItemResponseDto> Handle(CreateItemCommand request, CancellationToken ct)
    {
        var itemCode = await _repo.GenerateItemCodeAsync();
        var entity = new Models.Entities.Item
        {
            BaseName = request.Dto.BaseName,
            ItemCode = itemCode,
            TypeId = request.Dto.TypeId,
            CategoryId = request.Dto.CategoryId,
            UnitOfMeasureId = request.Dto.UnitOfMeasureId,
            Brand = request.Dto.Brand,
            Manufacturer = request.Dto.Manufacturer,
            Description = request.Dto.Description,
            ReorderLevel = request.Dto.ReorderLevel,
            StandardCost = request.Dto.StandardCost,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repo.CreateAsync(entity);
        return await BuildDto(created, ct);
    }

    private async Task<ItemResponseDto> BuildDto(Models.Entities.Item e, CancellationToken ct)
    {
        var type = await _context.Set<Models.Entities.InventoryType>().FindAsync(new object[] { e.TypeId }, ct);
        var category = await _context.Set<Models.Entities.InventoryCategory>().FindAsync(new object[] { e.CategoryId }, ct);
        var uom = await _context.Set<Models.Entities.UnitOfMeasure>().FindAsync(new object[] { e.UnitOfMeasureId }, ct);
        var variantCount = await _context.Set<Models.Entities.ItemVariant>().CountAsync(x => x.ItemId == e.Id && !x.IsDeleted, ct);

        return new ItemResponseDto
        {
            Id = e.Id, BaseName = e.BaseName, ItemCode = e.ItemCode,
            TypeId = e.TypeId, TypeName = type?.Name ?? string.Empty,
            CategoryId = e.CategoryId, CategoryName = category?.Name ?? string.Empty,
            UnitOfMeasureId = e.UnitOfMeasureId, UnitOfMeasureName = uom?.Name ?? string.Empty,
            UnitAbbreviation = uom?.Abbreviation ?? string.Empty,
            Brand = e.Brand, Manufacturer = e.Manufacturer, Description = e.Description,
            ReorderLevel = e.ReorderLevel, StandardCost = e.StandardCost,
            IsActive = e.IsActive, VariantCount = variantCount, CreatedAt = e.CreatedAt
        };
    }
}

public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, ItemResponseDto>
{
    private readonly IItemRepository _repo;
    private readonly AppDbContext _context;

    public UpdateItemCommandHandler(IItemRepository repo, AppDbContext context) { _repo = repo; _context = context; }

    public async Task<ItemResponseDto> Handle(UpdateItemCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Item), request.Id);

        entity.BaseName = request.Dto.BaseName;
        entity.TypeId = request.Dto.TypeId;
        entity.CategoryId = request.Dto.CategoryId;
        entity.UnitOfMeasureId = request.Dto.UnitOfMeasureId;
        entity.Brand = request.Dto.Brand;
        entity.Manufacturer = request.Dto.Manufacturer;
        entity.Description = request.Dto.Description;
        entity.ReorderLevel = request.Dto.ReorderLevel;
        entity.StandardCost = request.Dto.StandardCost;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(entity);

        var type = await _context.Set<Models.Entities.InventoryType>().FindAsync(new object[] { updated.TypeId }, ct);
        var category = await _context.Set<Models.Entities.InventoryCategory>().FindAsync(new object[] { updated.CategoryId }, ct);
        var uom = await _context.Set<Models.Entities.UnitOfMeasure>().FindAsync(new object[] { updated.UnitOfMeasureId }, ct);
        var variantCount = await _context.Set<Models.Entities.ItemVariant>().CountAsync(x => x.ItemId == updated.Id && !x.IsDeleted, ct);

        return new ItemResponseDto
        {
            Id = updated.Id, BaseName = updated.BaseName, ItemCode = updated.ItemCode,
            TypeId = updated.TypeId, TypeName = type?.Name ?? string.Empty,
            CategoryId = updated.CategoryId, CategoryName = category?.Name ?? string.Empty,
            UnitOfMeasureId = updated.UnitOfMeasureId, UnitOfMeasureName = uom?.Name ?? string.Empty,
            UnitAbbreviation = uom?.Abbreviation ?? string.Empty,
            Brand = updated.Brand, Manufacturer = updated.Manufacturer, Description = updated.Description,
            ReorderLevel = updated.ReorderLevel, StandardCost = updated.StandardCost,
            IsActive = updated.IsActive, VariantCount = variantCount, CreatedAt = updated.CreatedAt
        };
    }
}

public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, Unit>
{
    private readonly IItemRepository _repo;
    public DeleteItemCommandHandler(IItemRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteItemCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Item), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
