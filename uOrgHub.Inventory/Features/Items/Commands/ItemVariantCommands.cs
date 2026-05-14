using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Items.Commands;

public record CreateItemVariantCommand(CreateItemVariantDto Dto) : ICommand<ItemVariantResponseDto>;
public record UpdateItemVariantCommand(Guid Id, UpdateItemVariantDto Dto) : ICommand<ItemVariantResponseDto>;
public record DeleteItemVariantCommand(Guid Id) : ICommand<Unit>;

public class CreateItemVariantCommandHandler : IRequestHandler<CreateItemVariantCommand, ItemVariantResponseDto>
{
    private readonly IItemVariantRepository _repo;
    private readonly AppDbContext _context;

    public CreateItemVariantCommandHandler(IItemVariantRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<ItemVariantResponseDto> Handle(CreateItemVariantCommand request, CancellationToken ct)
    {
        var item = await _context.Set<Models.Entities.Item>().FindAsync(new object[] { request.Dto.ItemId }, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Item), request.Dto.ItemId);

        var sku = await _repo.GenerateSKUAsync(request.Dto.ItemId);
        var attributeHash = ComputeAttributeHash(request.Dto.Attributes);
        var variantName = await BuildVariantName(item.BaseName, request.Dto.Attributes, ct);

        var entity = new Models.Entities.ItemVariant
        {
            ItemId = request.Dto.ItemId,
            SKU = sku,
            VariantName = variantName,
            Barcode = request.Dto.Barcode,
            CostPrice = request.Dto.CostPrice,
            SellingPrice = request.Dto.SellingPrice,
            IsDefault = request.Dto.IsDefault,
            AttributeHash = attributeHash,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(entity);

        foreach (var attr in request.Dto.Attributes)
        {
            _context.Set<Models.Entities.VariantAttribute>().Add(new Models.Entities.VariantAttribute
            {
                ItemVariantId = created.Id,
                AttributeDefinitionId = attr.AttributeDefinitionId,
                Value = attr.Value,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync(ct);

        return await BuildDto(created, ct);
    }

    private static string ComputeAttributeHash(List<VariantAttributeValueDto> attributes)
    {
        var sorted = attributes.OrderBy(x => x.AttributeDefinitionId).Select(x => $"{x.AttributeDefinitionId}:{x.Value}");
        var input = string.Join("|", sorted);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }

    private async Task<string> BuildVariantName(string baseName, List<VariantAttributeValueDto> attributes, CancellationToken ct)
    {
        if (attributes.Count == 0) return baseName;
        var attrNames = new List<string>();
        foreach (var attr in attributes)
        {
            var def = await _context.Set<Models.Entities.AttributeDefinition>().FindAsync(new object[] { attr.AttributeDefinitionId }, ct);
            if (def != null) attrNames.Add(attr.Value);
        }
        return attrNames.Count > 0 ? $"{baseName} - {string.Join(" / ", attrNames)}" : baseName;
    }

    private async Task<ItemVariantResponseDto> BuildDto(Models.Entities.ItemVariant e, CancellationToken ct)
    {
        var item = await _context.Set<Models.Entities.Item>().FindAsync(new object[] { e.ItemId }, ct);
        var attrs = await _context.Set<Models.Entities.VariantAttribute>()
            .Include(x => x.AttributeDefinition)
            .Where(x => x.ItemVariantId == e.Id && !x.IsDeleted)
            .ToListAsync(ct);

        return new ItemVariantResponseDto
        {
            Id = e.Id, ItemId = e.ItemId, ItemBaseName = item?.BaseName ?? string.Empty,
            SKU = e.SKU, VariantName = e.VariantName, Barcode = e.Barcode,
            CostPrice = e.CostPrice, SellingPrice = e.SellingPrice,
            IsDefault = e.IsDefault, IsActive = e.IsActive, AttributeHash = e.AttributeHash,
            Attributes = attrs.Select(a => new VariantAttributeResponseDto
            {
                Id = a.Id, AttributeDefinitionId = a.AttributeDefinitionId,
                AttributeName = a.AttributeDefinition?.Name ?? string.Empty, Value = a.Value
            }).ToList(),
            CreatedAt = e.CreatedAt
        };
    }
}

public class UpdateItemVariantCommandHandler : IRequestHandler<UpdateItemVariantCommand, ItemVariantResponseDto>
{
    private readonly IItemVariantRepository _repo;
    private readonly AppDbContext _context;

    public UpdateItemVariantCommandHandler(IItemVariantRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<ItemVariantResponseDto> Handle(UpdateItemVariantCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.ItemVariant), request.Id);

        var item = await _context.Set<Models.Entities.Item>().FindAsync(new object[] { entity.ItemId }, ct);

        entity.Barcode = request.Dto.Barcode;
        entity.CostPrice = request.Dto.CostPrice;
        entity.SellingPrice = request.Dto.SellingPrice;
        entity.IsDefault = request.Dto.IsDefault;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        if (request.Dto.Attributes.Count > 0)
        {
            var sorted = request.Dto.Attributes.OrderBy(x => x.AttributeDefinitionId).Select(x => $"{x.AttributeDefinitionId}:{x.Value}");
            var input = string.Join("|", sorted);
            entity.AttributeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)))[..16];

            var attrNames = new List<string>();
            foreach (var attr in request.Dto.Attributes)
            {
                var def = await _context.Set<Models.Entities.AttributeDefinition>().FindAsync(new object[] { attr.AttributeDefinitionId }, ct);
                if (def != null) attrNames.Add(attr.Value);
            }
            entity.VariantName = attrNames.Count > 0 ? $"{item?.BaseName} - {string.Join(" / ", attrNames)}" : (item?.BaseName ?? entity.VariantName);

            var existingAttrs = await _context.Set<Models.Entities.VariantAttribute>()
                .Where(x => x.ItemVariantId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var ea in existingAttrs) { ea.IsDeleted = true; ea.DeletedAt = DateTime.UtcNow; }

            foreach (var attr in request.Dto.Attributes)
            {
                _context.Set<Models.Entities.VariantAttribute>().Add(new Models.Entities.VariantAttribute
                {
                    ItemVariantId = entity.Id,
                    AttributeDefinitionId = attr.AttributeDefinitionId,
                    Value = attr.Value,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _repo.UpdateAsync(entity);
        await _context.SaveChangesAsync(ct);

        var attrs = await _context.Set<Models.Entities.VariantAttribute>()
            .Include(x => x.AttributeDefinition)
            .Where(x => x.ItemVariantId == entity.Id && !x.IsDeleted)
            .ToListAsync(ct);

        return new ItemVariantResponseDto
        {
            Id = entity.Id, ItemId = entity.ItemId, ItemBaseName = item?.BaseName ?? string.Empty,
            SKU = entity.SKU, VariantName = entity.VariantName, Barcode = entity.Barcode,
            CostPrice = entity.CostPrice, SellingPrice = entity.SellingPrice,
            IsDefault = entity.IsDefault, IsActive = entity.IsActive, AttributeHash = entity.AttributeHash,
            Attributes = attrs.Select(a => new VariantAttributeResponseDto
            {
                Id = a.Id, AttributeDefinitionId = a.AttributeDefinitionId,
                AttributeName = a.AttributeDefinition?.Name ?? string.Empty, Value = a.Value
            }).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }
}

public class DeleteItemVariantCommandHandler : IRequestHandler<DeleteItemVariantCommand, Unit>
{
    private readonly IItemVariantRepository _repo;
    public DeleteItemVariantCommandHandler(IItemVariantRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteItemVariantCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.ItemVariant), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
