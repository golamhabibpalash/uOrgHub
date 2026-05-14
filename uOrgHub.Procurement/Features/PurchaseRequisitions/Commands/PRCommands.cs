using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.PurchaseRequisitions.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.PurchaseRequisitions.Commands;

public record CreatePRCommand(CreatePRDto Dto) : ICommand<PRResponseDto>;
public record UpdatePRCommand(Guid Id, UpdatePRDto Dto) : ICommand<PRResponseDto>;
public record DeletePRCommand(Guid Id) : ICommand<Unit>;
public record SubmitPRCommand(Guid Id) : ICommand<PRResponseDto>;
public record ApprovePRCommand(Guid Id) : ICommand<PRResponseDto>;
public record RejectPRCommand(Guid Id, RejectPRDto Dto) : ICommand<PRResponseDto>;

public class CreatePRCommandHandler : IRequestHandler<CreatePRCommand, PRResponseDto>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly AppDbContext _context;

    public CreatePRCommandHandler(IPurchaseRequisitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<PRResponseDto> Handle(CreatePRCommand request, CancellationToken ct)
    {
        var prNumber = await _repo.GeneratePRNumberAsync();
        var entity = new PurchaseRequisition
        {
            PRNumber = prNumber,
            PRDate = request.Dto.PRDate,
            RequiredDate = request.Dto.RequiredDate,
            DepartmentId = request.Dto.DepartmentId,
            RequestedById = request.Dto.RequestedById,
            Purpose = request.Dto.Purpose,
            Notes = request.Dto.Notes,
            Status = PRStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Items = request.Dto.Items.Select(i => new PurchaseRequisitionItem
            {
                ItemVariantId = i.ItemVariantId,
                WarehouseId = i.WarehouseId,
                RequestedQuantity = i.RequestedQuantity,
                EstimatedUnitCost = i.EstimatedUnitCost,
                EstimatedTotalCost = i.RequestedQuantity * i.EstimatedUnitCost,
                Notes = i.Notes,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };
        var created = await _repo.CreateAsync(entity);
        return await BuildResponseAsync(created, ct);
    }

    private async Task<PRResponseDto> BuildResponseAsync(PurchaseRequisition pr, CancellationToken ct)
    {
        var empIds = new List<Guid> { pr.RequestedById };
        if (pr.ApprovedById.HasValue) empIds.Add(pr.ApprovedById.Value);
        var variantIds = pr.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouseIds = pr.Items.Select(x => x.WarehouseId).Distinct().ToList();

        var depts = await _context.Set<Department>().Where(x => x.Id == pr.DepartmentId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var emps = await _context.Set<Employee>().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);
        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return GetPRsQueryHandler.BuildDto(pr, depts, emps, variants, warehouses);
    }
}

public class UpdatePRCommandHandler : IRequestHandler<UpdatePRCommand, PRResponseDto>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly AppDbContext _context;

    public UpdatePRCommandHandler(IPurchaseRequisitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<PRResponseDto> Handle(UpdatePRCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        if (entity.Status != PRStatus.Draft)
            throw new AppException("Only Draft purchase requisitions can be updated.");

        entity.PRDate = request.Dto.PRDate;
        entity.RequiredDate = request.Dto.RequiredDate;
        entity.DepartmentId = request.Dto.DepartmentId;
        entity.RequestedById = request.Dto.RequestedById;
        entity.Purpose = request.Dto.Purpose;
        entity.Notes = request.Dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        var existingItemIds = entity.Items.Select(x => x.Id).ToList();
        var incomingItemIds = request.Dto.Items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var toRemove = entity.Items.Where(x => !incomingItemIds.Contains(x.Id)).ToList();
        foreach (var item in toRemove)
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
                    existing.ItemVariantId = itemDto.ItemVariantId;
                    existing.WarehouseId = itemDto.WarehouseId;
                    existing.RequestedQuantity = itemDto.RequestedQuantity;
                    existing.EstimatedUnitCost = itemDto.EstimatedUnitCost;
                    existing.EstimatedTotalCost = itemDto.RequestedQuantity * itemDto.EstimatedUnitCost;
                    existing.Notes = itemDto.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                entity.Items.Add(new PurchaseRequisitionItem
                {
                    ItemVariantId = itemDto.ItemVariantId,
                    WarehouseId = itemDto.WarehouseId,
                    RequestedQuantity = itemDto.RequestedQuantity,
                    EstimatedUnitCost = itemDto.EstimatedUnitCost,
                    EstimatedTotalCost = itemDto.RequestedQuantity * itemDto.EstimatedUnitCost,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var updated = await _repo.UpdateAsync(entity);

        var empIds = new List<Guid> { updated.RequestedById };
        if (updated.ApprovedById.HasValue) empIds.Add(updated.ApprovedById.Value);
        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouseIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.WarehouseId).Distinct().ToList();

        var depts = await _context.Set<Department>().Where(x => x.Id == updated.DepartmentId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var emps = await _context.Set<Employee>().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);
        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return GetPRsQueryHandler.BuildDto(updated, depts, emps, variants, warehouses);
    }
}

public class DeletePRCommandHandler : IRequestHandler<DeletePRCommand, Unit>
{
    private readonly IPurchaseRequisitionRepository _repo;
    public DeletePRCommandHandler(IPurchaseRequisitionRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeletePRCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(PurchaseRequisition), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}

public class SubmitPRCommandHandler : IRequestHandler<SubmitPRCommand, PRResponseDto>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly AppDbContext _context;

    public SubmitPRCommandHandler(IPurchaseRequisitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<PRResponseDto> Handle(SubmitPRCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        if (entity.Status != PRStatus.Draft)
            throw new AppException("Only Draft purchase requisitions can be submitted.");

        entity.Status = PRStatus.Submitted;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);

        var depts = await _context.Set<Department>().Where(x => x.Id == updated.DepartmentId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var emps = await _context.Set<Employee>().Where(x => x.Id == updated.RequestedById).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouseIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.WarehouseId).Distinct().ToList();
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);
        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return GetPRsQueryHandler.BuildDto(updated, depts, emps, variants, warehouses);
    }
}

public class ApprovePRCommandHandler : IRequestHandler<ApprovePRCommand, PRResponseDto>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly AppDbContext _context;

    public ApprovePRCommandHandler(IPurchaseRequisitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<PRResponseDto> Handle(ApprovePRCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        if (entity.Status != PRStatus.Submitted)
            throw new AppException("Only Submitted purchase requisitions can be approved.");

        entity.Status = PRStatus.Approved;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);

        var empIds = new List<Guid> { updated.RequestedById };
        if (updated.ApprovedById.HasValue) empIds.Add(updated.ApprovedById.Value);
        var depts = await _context.Set<Department>().Where(x => x.Id == updated.DepartmentId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var emps = await _context.Set<Employee>().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouseIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.WarehouseId).Distinct().ToList();
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);
        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return GetPRsQueryHandler.BuildDto(updated, depts, emps, variants, warehouses);
    }
}

public class RejectPRCommandHandler : IRequestHandler<RejectPRCommand, PRResponseDto>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly AppDbContext _context;

    public RejectPRCommandHandler(IPurchaseRequisitionRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<PRResponseDto> Handle(RejectPRCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        if (entity.Status != PRStatus.Submitted)
            throw new AppException("Only Submitted purchase requisitions can be rejected.");

        entity.Status = PRStatus.Rejected;
        entity.RejectionReason = request.Dto.Reason;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);

        var depts = await _context.Set<Department>().Where(x => x.Id == updated.DepartmentId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var emps = await _context.Set<Employee>().Where(x => x.Id == updated.RequestedById).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouseIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.WarehouseId).Distinct().ToList();
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);
        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return GetPRsQueryHandler.BuildDto(updated, depts, emps, variants, warehouses);
    }
}
