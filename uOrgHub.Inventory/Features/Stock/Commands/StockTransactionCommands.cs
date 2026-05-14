using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Stock.Commands;

public record CreateStockTransactionCommand(CreateStockTransactionDto Dto) : ICommand<StockTransactionResponseDto>;
public record UpdateStockTransactionCommand(Guid Id, UpdateStockTransactionDto Dto) : ICommand<StockTransactionResponseDto>;
public record DeleteStockTransactionCommand(Guid Id) : ICommand<Unit>;
public record ConfirmStockTransactionCommand(Guid Id) : ICommand<StockTransactionResponseDto>;
public record CancelStockTransactionCommand(Guid Id) : ICommand<StockTransactionResponseDto>;

public class CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, StockTransactionResponseDto>
{
    private readonly IStockTransactionRepository _repo;
    private readonly AppDbContext _context;

    public CreateStockTransactionCommandHandler(IStockTransactionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<StockTransactionResponseDto> Handle(CreateStockTransactionCommand request, CancellationToken ct)
    {
        var txnNumber = await _repo.GenerateTransactionNumberAsync();
        var entity = new Models.Entities.StockTransaction
        {
            TransactionNumber = txnNumber,
            TransactionDate = request.Dto.TransactionDate,
            TransactionType = request.Dto.TransactionType,
            Status = StockTransactionStatus.Draft,
            ItemVariantId = request.Dto.ItemVariantId,
            WarehouseId = request.Dto.WarehouseId,
            FromWarehouseId = request.Dto.FromWarehouseId,
            Quantity = request.Dto.Quantity,
            UnitCost = request.Dto.UnitCost,
            ReferenceNumber = request.Dto.ReferenceNumber,
            Notes = request.Dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repo.CreateAsync(entity);
        return await BuildDto(created, ct);
    }

    private async Task<StockTransactionResponseDto> BuildDto(Models.Entities.StockTransaction e, CancellationToken ct)
    {
        var variant = await _context.Set<Models.Entities.ItemVariant>().FindAsync(new object[] { e.ItemVariantId }, ct);
        var warehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { e.WarehouseId }, ct);
        Models.Entities.Warehouse? fromWarehouse = null;
        if (e.FromWarehouseId.HasValue)
            fromWarehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { e.FromWarehouseId.Value }, ct);

        return new StockTransactionResponseDto
        {
            Id = e.Id, TransactionNumber = e.TransactionNumber, TransactionDate = e.TransactionDate,
            TransactionType = e.TransactionType, Status = e.Status,
            ItemVariantId = e.ItemVariantId, VariantSKU = variant?.SKU ?? string.Empty, VariantName = variant?.VariantName ?? string.Empty,
            WarehouseId = e.WarehouseId, WarehouseName = warehouse?.Name ?? string.Empty,
            FromWarehouseId = e.FromWarehouseId, FromWarehouseName = fromWarehouse?.Name,
            Quantity = e.Quantity, UnitCost = e.UnitCost,
            ReferenceNumber = e.ReferenceNumber, Notes = e.Notes, CreatedAt = e.CreatedAt
        };
    }
}

public class UpdateStockTransactionCommandHandler : IRequestHandler<UpdateStockTransactionCommand, StockTransactionResponseDto>
{
    private readonly IStockTransactionRepository _repo;
    private readonly AppDbContext _context;

    public UpdateStockTransactionCommandHandler(IStockTransactionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<StockTransactionResponseDto> Handle(UpdateStockTransactionCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.StockTransaction), request.Id);

        if (entity.Status != StockTransactionStatus.Draft)
            throw new AppException("Only Draft transactions can be updated.");

        entity.TransactionDate = request.Dto.TransactionDate;
        entity.ItemVariantId = request.Dto.ItemVariantId;
        entity.WarehouseId = request.Dto.WarehouseId;
        entity.FromWarehouseId = request.Dto.FromWarehouseId;
        entity.Quantity = request.Dto.Quantity;
        entity.UnitCost = request.Dto.UnitCost;
        entity.ReferenceNumber = request.Dto.ReferenceNumber;
        entity.Notes = request.Dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(entity);

        var variant = await _context.Set<Models.Entities.ItemVariant>().FindAsync(new object[] { updated.ItemVariantId }, ct);
        var warehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { updated.WarehouseId }, ct);
        Models.Entities.Warehouse? fromWarehouse = null;
        if (updated.FromWarehouseId.HasValue)
            fromWarehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { updated.FromWarehouseId.Value }, ct);

        return new StockTransactionResponseDto
        {
            Id = updated.Id, TransactionNumber = updated.TransactionNumber, TransactionDate = updated.TransactionDate,
            TransactionType = updated.TransactionType, Status = updated.Status,
            ItemVariantId = updated.ItemVariantId, VariantSKU = variant?.SKU ?? string.Empty, VariantName = variant?.VariantName ?? string.Empty,
            WarehouseId = updated.WarehouseId, WarehouseName = warehouse?.Name ?? string.Empty,
            FromWarehouseId = updated.FromWarehouseId, FromWarehouseName = fromWarehouse?.Name,
            Quantity = updated.Quantity, UnitCost = updated.UnitCost,
            ReferenceNumber = updated.ReferenceNumber, Notes = updated.Notes, CreatedAt = updated.CreatedAt
        };
    }
}

public class DeleteStockTransactionCommandHandler : IRequestHandler<DeleteStockTransactionCommand, Unit>
{
    private readonly IStockTransactionRepository _repo;
    public DeleteStockTransactionCommandHandler(IStockTransactionRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteStockTransactionCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.StockTransaction), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}

public class ConfirmStockTransactionCommandHandler : IRequestHandler<ConfirmStockTransactionCommand, StockTransactionResponseDto>
{
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IStockBalanceRepository _balanceRepo;
    private readonly AppDbContext _context;

    public ConfirmStockTransactionCommandHandler(IStockTransactionRepository txnRepo, IStockBalanceRepository balanceRepo, AppDbContext context)
    { _txnRepo = txnRepo; _balanceRepo = balanceRepo; _context = context; }

    public async Task<StockTransactionResponseDto> Handle(ConfirmStockTransactionCommand request, CancellationToken ct)
    {
        var entity = await _txnRepo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.StockTransaction), request.Id);

        if (entity.Status != StockTransactionStatus.Draft)
            throw new AppException("Only Draft transactions can be confirmed.");

        await UpdateStockBalances(entity, ct);

        entity.Status = StockTransactionStatus.Confirmed;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _txnRepo.UpdateAsync(entity);

        var variant = await _context.Set<Models.Entities.ItemVariant>().FindAsync(new object[] { updated.ItemVariantId }, ct);
        var warehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { updated.WarehouseId }, ct);
        Models.Entities.Warehouse? fromWarehouse = null;
        if (updated.FromWarehouseId.HasValue)
            fromWarehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { updated.FromWarehouseId.Value }, ct);

        return new StockTransactionResponseDto
        {
            Id = updated.Id, TransactionNumber = updated.TransactionNumber, TransactionDate = updated.TransactionDate,
            TransactionType = updated.TransactionType, Status = updated.Status,
            ItemVariantId = updated.ItemVariantId, VariantSKU = variant?.SKU ?? string.Empty, VariantName = variant?.VariantName ?? string.Empty,
            WarehouseId = updated.WarehouseId, WarehouseName = warehouse?.Name ?? string.Empty,
            FromWarehouseId = updated.FromWarehouseId, FromWarehouseName = fromWarehouse?.Name,
            Quantity = updated.Quantity, UnitCost = updated.UnitCost,
            ReferenceNumber = updated.ReferenceNumber, Notes = updated.Notes, CreatedAt = updated.CreatedAt
        };
    }

    private async Task UpdateStockBalances(Models.Entities.StockTransaction txn, CancellationToken ct)
    {
        switch (txn.TransactionType)
        {
            case StockTransactionType.GoodsReceived:
            case StockTransactionType.Return:
            {
                var balance = await _balanceRepo.GetOrCreateAsync(txn.ItemVariantId, txn.WarehouseId);
                balance.QuantityOnHand += txn.Quantity;
                balance.LastUpdated = DateTime.UtcNow;
                await _balanceRepo.UpdateAsync(balance);
                break;
            }
            case StockTransactionType.GoodsIssued:
            {
                var balance = await _balanceRepo.GetOrCreateAsync(txn.ItemVariantId, txn.WarehouseId);
                balance.QuantityOnHand -= txn.Quantity;
                balance.LastUpdated = DateTime.UtcNow;
                await _balanceRepo.UpdateAsync(balance);
                break;
            }
            case StockTransactionType.Transfer:
            {
                if (!txn.FromWarehouseId.HasValue)
                    throw new AppException("FromWarehouseId is required for Transfer transactions.");

                var fromBalance = await _balanceRepo.GetOrCreateAsync(txn.ItemVariantId, txn.FromWarehouseId.Value);
                fromBalance.QuantityOnHand -= txn.Quantity;
                fromBalance.LastUpdated = DateTime.UtcNow;
                await _balanceRepo.UpdateAsync(fromBalance);

                var toBalance = await _balanceRepo.GetOrCreateAsync(txn.ItemVariantId, txn.WarehouseId);
                toBalance.QuantityOnHand += txn.Quantity;
                toBalance.LastUpdated = DateTime.UtcNow;
                await _balanceRepo.UpdateAsync(toBalance);
                break;
            }
            case StockTransactionType.Adjustment:
            {
                var balance = await _balanceRepo.GetOrCreateAsync(txn.ItemVariantId, txn.WarehouseId);
                balance.QuantityOnHand += txn.Quantity;
                balance.LastUpdated = DateTime.UtcNow;
                await _balanceRepo.UpdateAsync(balance);
                break;
            }
        }
    }
}

public class CancelStockTransactionCommandHandler : IRequestHandler<CancelStockTransactionCommand, StockTransactionResponseDto>
{
    private readonly IStockTransactionRepository _repo;
    private readonly AppDbContext _context;

    public CancelStockTransactionCommandHandler(IStockTransactionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<StockTransactionResponseDto> Handle(CancelStockTransactionCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.StockTransaction), request.Id);

        if (entity.Status == StockTransactionStatus.Cancelled)
            throw new AppException("Transaction is already cancelled.");

        if (entity.Status == StockTransactionStatus.Confirmed)
            throw new AppException("Confirmed transactions cannot be cancelled. Create a reversal transaction instead.");

        entity.Status = StockTransactionStatus.Cancelled;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);

        var variant = await _context.Set<Models.Entities.ItemVariant>().FindAsync(new object[] { updated.ItemVariantId }, ct);
        var warehouse = await _context.Set<Models.Entities.Warehouse>().FindAsync(new object[] { updated.WarehouseId }, ct);

        return new StockTransactionResponseDto
        {
            Id = updated.Id, TransactionNumber = updated.TransactionNumber, TransactionDate = updated.TransactionDate,
            TransactionType = updated.TransactionType, Status = updated.Status,
            ItemVariantId = updated.ItemVariantId, VariantSKU = variant?.SKU ?? string.Empty, VariantName = variant?.VariantName ?? string.Empty,
            WarehouseId = updated.WarehouseId, WarehouseName = warehouse?.Name ?? string.Empty,
            Quantity = updated.Quantity, UnitCost = updated.UnitCost,
            ReferenceNumber = updated.ReferenceNumber, Notes = updated.Notes, CreatedAt = updated.CreatedAt
        };
    }
}
