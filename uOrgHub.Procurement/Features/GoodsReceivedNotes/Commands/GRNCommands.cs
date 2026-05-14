using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.GoodsReceivedNotes.Queries;
using uOrgHub.Procurement.Features.PurchaseOrders.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.GoodsReceivedNotes.Commands;

public record CreateGRNCommand(CreateGRNDto Dto) : ICommand<GRNResponseDto>;
public record UpdateGRNCommand(Guid Id, UpdateGRNDto Dto) : ICommand<GRNResponseDto>;
public record DeleteGRNCommand(Guid Id) : ICommand<Unit>;
public record ConfirmGRNCommand(Guid Id) : ICommand<GRNResponseDto>;

public class CreateGRNCommandHandler : IRequestHandler<CreateGRNCommand, GRNResponseDto>
{
    private readonly IGoodsReceivedNoteRepository _repo;
    private readonly AppDbContext _context;

    public CreateGRNCommandHandler(IGoodsReceivedNoteRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<GRNResponseDto> Handle(CreateGRNCommand request, CancellationToken ct)
    {
        var po = await _context.Set<PurchaseOrder>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted && x.Id == request.Dto.POId).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Dto.POId);

        if (po.Status != POStatus.Confirmed && po.Status != POStatus.PartiallyReceived)
            throw new AppException("GRN can only be created for Confirmed or PartiallyReceived purchase orders.");

        var grnNumber = await _repo.GenerateGRNNumberAsync();
        var items = request.Dto.Items.Select(i => new GRNItem
        {
            POItemId = i.POItemId,
            ItemVariantId = i.ItemVariantId,
            OrderedQuantity = i.OrderedQuantity,
            ReceivedQuantity = i.ReceivedQuantity,
            RejectedQuantity = i.RejectedQuantity,
            AcceptedQuantity = i.ReceivedQuantity - i.RejectedQuantity,
            UnitCost = i.UnitCost,
            Notes = i.Notes,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var entity = new GoodsReceivedNote
        {
            GRNNumber = grnNumber,
            GRNDate = request.Dto.GRNDate,
            POId = request.Dto.POId,
            WarehouseId = request.Dto.WarehouseId,
            ReceivedById = request.Dto.ReceivedById,
            Status = GRNStatus.Draft,
            Notes = request.Dto.Notes,
            InvoiceNumber = request.Dto.InvoiceNumber,
            InvoiceDate = request.Dto.InvoiceDate,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };
        var created = await _repo.CreateAsync(entity);
        return await BuildResponseAsync(created, ct);
    }

    private async Task<GRNResponseDto> BuildResponseAsync(GoodsReceivedNote grn, CancellationToken ct)
    {
        var po = await _context.Set<PurchaseOrder>().FindAsync(new object[] { grn.POId }, ct);
        grn.PurchaseOrder = po!;
        var variantIds = grn.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouses = await _context.Set<Warehouse>().Where(x => x.Id == grn.WarehouseId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var receivers = await _context.Set<Employee>().Where(x => x.Id == grn.ReceivedById).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GRNQueryHelper.BuildDto(grn, warehouses, receivers, variants);
    }
}

public class UpdateGRNCommandHandler : IRequestHandler<UpdateGRNCommand, GRNResponseDto>
{
    private readonly IGoodsReceivedNoteRepository _repo;
    private readonly AppDbContext _context;

    public UpdateGRNCommandHandler(IGoodsReceivedNoteRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<GRNResponseDto> Handle(UpdateGRNCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(GoodsReceivedNote), request.Id);

        if (entity.Status != GRNStatus.Draft)
            throw new AppException("Only Draft GRNs can be updated.");

        entity.GRNDate = request.Dto.GRNDate;
        entity.WarehouseId = request.Dto.WarehouseId;
        entity.ReceivedById = request.Dto.ReceivedById;
        entity.Notes = request.Dto.Notes;
        entity.InvoiceNumber = request.Dto.InvoiceNumber;
        entity.InvoiceDate = request.Dto.InvoiceDate;
        entity.UpdatedAt = DateTime.UtcNow;

        var incomingItemIds = request.Dto.Items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        foreach (var item in entity.Items.Where(x => !incomingItemIds.Contains(x.Id)))
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
        }
        foreach (var itemDto in request.Dto.Items)
        {
            if (itemDto.Id.HasValue)
            {
                var existing = entity.Items.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                if (existing != null)
                {
                    existing.POItemId = itemDto.POItemId;
                    existing.ItemVariantId = itemDto.ItemVariantId;
                    existing.OrderedQuantity = itemDto.OrderedQuantity;
                    existing.ReceivedQuantity = itemDto.ReceivedQuantity;
                    existing.RejectedQuantity = itemDto.RejectedQuantity;
                    existing.AcceptedQuantity = itemDto.ReceivedQuantity - itemDto.RejectedQuantity;
                    existing.UnitCost = itemDto.UnitCost;
                    existing.Notes = itemDto.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                entity.Items.Add(new GRNItem
                {
                    POItemId = itemDto.POItemId,
                    ItemVariantId = itemDto.ItemVariantId,
                    OrderedQuantity = itemDto.OrderedQuantity,
                    ReceivedQuantity = itemDto.ReceivedQuantity,
                    RejectedQuantity = itemDto.RejectedQuantity,
                    AcceptedQuantity = itemDto.ReceivedQuantity - itemDto.RejectedQuantity,
                    UnitCost = itemDto.UnitCost,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var updated = await _repo.UpdateAsync(entity);
        return await BuildResponseAsync(updated, ct);
    }

    private async Task<GRNResponseDto> BuildResponseAsync(GoodsReceivedNote grn, CancellationToken ct)
    {
        var po = await _context.Set<PurchaseOrder>().FindAsync(new object[] { grn.POId }, ct);
        grn.PurchaseOrder = po!;
        var variantIds = grn.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouses = await _context.Set<Warehouse>().Where(x => x.Id == grn.WarehouseId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var receivers = await _context.Set<Employee>().Where(x => x.Id == grn.ReceivedById).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GRNQueryHelper.BuildDto(grn, warehouses, receivers, variants);
    }
}

public class DeleteGRNCommandHandler : IRequestHandler<DeleteGRNCommand, Unit>
{
    private readonly IGoodsReceivedNoteRepository _repo;
    public DeleteGRNCommandHandler(IGoodsReceivedNoteRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteGRNCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(GoodsReceivedNote), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}

public class ConfirmGRNCommandHandler : IRequestHandler<ConfirmGRNCommand, GRNResponseDto>
{
    private readonly IGoodsReceivedNoteRepository _repo;
    private readonly AppDbContext _context;

    public ConfirmGRNCommandHandler(IGoodsReceivedNoteRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<GRNResponseDto> Handle(ConfirmGRNCommand request, CancellationToken ct)
    {
        var grn = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(GoodsReceivedNote), request.Id);

        if (grn.Status != GRNStatus.Draft)
            throw new AppException("Only Draft GRNs can be confirmed.");

        var po = await _context.Set<PurchaseOrder>()
            .Include(x => x.Items)
            .Where(x => x.Id == grn.POId).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(PurchaseOrder), grn.POId);

        var activeGrnItems = grn.Items.Where(x => !x.IsDeleted).ToList();

        foreach (var grnItem in activeGrnItems)
        {
            var poItem = po.Items.FirstOrDefault(x => x.Id == grnItem.POItemId && !x.IsDeleted);
            if (poItem != null)
            {
                var remaining = poItem.OrderedQuantity - poItem.ReceivedQuantity;
                if (grnItem.AcceptedQuantity > remaining)
                    throw new AppException($"Accepted quantity ({grnItem.AcceptedQuantity}) exceeds remaining ordered quantity ({remaining}) for item.");

                poItem.ReceivedQuantity += grnItem.AcceptedQuantity;
                poItem.UpdatedAt = DateTime.UtcNow;
            }

            await CreateStockTransactionAsync(grnItem, grn.WarehouseId, grn.GRNDate, grn.GRNNumber, ct);
        }

        var allItemsFullyReceived = po.Items.Where(x => !x.IsDeleted).All(x => x.ReceivedQuantity >= x.OrderedQuantity);
        po.Status = allItemsFullyReceived ? POStatus.FullyReceived : POStatus.PartiallyReceived;
        po.UpdatedAt = DateTime.UtcNow;
        _context.Set<PurchaseOrder>().Update(po);

        grn.Status = GRNStatus.Confirmed;
        grn.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(grn);

        await _context.SaveChangesAsync(ct);

        return await BuildResponseAsync(updated, ct);
    }

    private async Task CreateStockTransactionAsync(GRNItem grnItem, Guid warehouseId, DateTime date, string grnNumber, CancellationToken ct)
    {
        var txnCount = await _context.Set<StockTransaction>().CountAsync(ct) + 1;
        var txnNumber = $"TXN-{DateTime.UtcNow:yyyyMM}-{txnCount:D5}";

        var stockTxn = new StockTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = txnNumber,
            TransactionDate = date,
            TransactionType = StockTransactionType.GoodsReceived,
            Status = StockTransactionStatus.Confirmed,
            ItemVariantId = grnItem.ItemVariantId,
            WarehouseId = warehouseId,
            Quantity = grnItem.AcceptedQuantity,
            UnitCost = grnItem.UnitCost,
            ReferenceNumber = grnNumber,
            Notes = $"GRN: {grnNumber}",
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<StockTransaction>().Add(stockTxn);

        var balance = await _context.Set<StockBalance>()
            .Where(x => x.ItemVariantId == grnItem.ItemVariantId && x.WarehouseId == warehouseId && !x.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (balance == null)
        {
            balance = new StockBalance
            {
                Id = Guid.NewGuid(),
                ItemVariantId = grnItem.ItemVariantId,
                WarehouseId = warehouseId,
                QuantityOnHand = grnItem.AcceptedQuantity,
                QuantityReserved = 0,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<StockBalance>().Add(balance);
        }
        else
        {
            balance.QuantityOnHand += grnItem.AcceptedQuantity;
            balance.LastUpdated = DateTime.UtcNow;
            balance.UpdatedAt = DateTime.UtcNow;
            _context.Set<StockBalance>().Update(balance);
        }
    }

    private async Task<GRNResponseDto> BuildResponseAsync(GoodsReceivedNote grn, CancellationToken ct)
    {
        var po = await _context.Set<PurchaseOrder>().FindAsync(new object[] { grn.POId }, ct);
        grn.PurchaseOrder = po!;
        var variantIds = grn.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouses = await _context.Set<Warehouse>().Where(x => x.Id == grn.WarehouseId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var receivers = await _context.Set<Employee>().Where(x => x.Id == grn.ReceivedById).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GRNQueryHelper.BuildDto(grn, warehouses, receivers, variants);
    }
}
