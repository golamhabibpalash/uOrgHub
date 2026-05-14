using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.PurchaseOrders.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.PurchaseOrders.Commands;

public record CreatePOCommand(CreatePurchaseOrderDto Dto) : ICommand<POResponseDto>;
public record UpdatePOCommand(Guid Id, UpdatePurchaseOrderDto Dto) : ICommand<POResponseDto>;
public record DeletePOCommand(Guid Id) : ICommand<Unit>;
public record SendPOCommand(Guid Id) : ICommand<POResponseDto>;
public record ConfirmPOCommand(Guid Id) : ICommand<POResponseDto>;
public record CancelPOCommand(Guid Id) : ICommand<POResponseDto>;

public class CreatePOCommandHandler : IRequestHandler<CreatePOCommand, POResponseDto>
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly AppDbContext _context;

    public CreatePOCommandHandler(IPurchaseOrderRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<POResponseDto> Handle(CreatePOCommand request, CancellationToken ct)
    {
        var vendor = await _context.Set<Vendor>().Where(x => !x.IsDeleted && x.Id == request.Dto.VendorId).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Vendor), request.Dto.VendorId);

        if (vendor.Status == VendorStatus.Blacklisted)
            throw new AppException("Blacklisted vendors cannot be assigned to purchase orders.");

        var poNumber = await _repo.GeneratePONumberAsync();
        var items = request.Dto.Items.Select(i =>
        {
            var lineTotal = i.OrderedQuantity * i.UnitPrice;
            var taxAmt = lineTotal * i.TaxPercent / 100;
            var discAmt = lineTotal * i.DiscountPercent / 100;
            return new PurchaseOrderItem
            {
                ItemVariantId = i.ItemVariantId,
                OrderedQuantity = i.OrderedQuantity,
                ReceivedQuantity = 0,
                UnitPrice = i.UnitPrice,
                TaxPercent = i.TaxPercent,
                DiscountPercent = i.DiscountPercent,
                TotalPrice = lineTotal + taxAmt - discAmt,
                Notes = i.Notes,
                CreatedAt = DateTime.UtcNow
            };
        }).ToList();

        var subTotal = items.Sum(x => x.OrderedQuantity * x.UnitPrice);
        var taxAmount = items.Sum(x => x.OrderedQuantity * x.UnitPrice * x.TaxPercent / 100);
        var discountAmount = items.Sum(x => x.OrderedQuantity * x.UnitPrice * x.DiscountPercent / 100);

        var entity = new PurchaseOrder
        {
            PONumber = poNumber,
            PODate = request.Dto.PODate,
            ExpectedDeliveryDate = request.Dto.ExpectedDeliveryDate,
            VendorId = request.Dto.VendorId,
            QuotationId = request.Dto.QuotationId,
            PRId = request.Dto.PRId,
            Status = POStatus.Draft,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            TotalAmount = subTotal + taxAmount - discountAmount,
            PaymentTerms = request.Dto.PaymentTerms,
            DeliveryAddress = request.Dto.DeliveryAddress,
            Notes = request.Dto.Notes,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };
        var created = await _repo.CreateAsync(entity);
        return await BuildResponseAsync(created, ct);
    }

    private async Task<POResponseDto> BuildResponseAsync(PurchaseOrder po, CancellationToken ct)
    {
        var quotationIds = po.QuotationId.HasValue ? new List<Guid> { po.QuotationId.Value } : new List<Guid>();
        var prIds = po.PRId.HasValue ? new List<Guid> { po.PRId.Value } : new List<Guid>();
        var approverIds = po.ApprovedById.HasValue ? new List<Guid> { po.ApprovedById.Value } : new List<Guid>();
        var variantIds = po.Items.Select(x => x.ItemVariantId).Distinct().ToList();

        var vendor = await _context.Set<Vendor>().FindAsync(new object[] { po.VendorId }, ct);
        po.Vendor = vendor!;

        var quotations = await _context.Set<VendorQuotation>().Where(x => quotationIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.QuotationNumber, ct);
        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var approvers = await _context.Set<Employee>().Where(x => approverIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return GetPOsQueryHandler.BuildDto(po, quotations, prs, approvers, variants);
    }
}

public class UpdatePOCommandHandler : IRequestHandler<UpdatePOCommand, POResponseDto>
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly AppDbContext _context;

    public UpdatePOCommandHandler(IPurchaseOrderRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<POResponseDto> Handle(UpdatePOCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (entity.Status != POStatus.Draft)
            throw new AppException("Only Draft purchase orders can be updated.");

        entity.PODate = request.Dto.PODate;
        entity.ExpectedDeliveryDate = request.Dto.ExpectedDeliveryDate;
        entity.VendorId = request.Dto.VendorId;
        entity.QuotationId = request.Dto.QuotationId;
        entity.PRId = request.Dto.PRId;
        entity.PaymentTerms = request.Dto.PaymentTerms;
        entity.DeliveryAddress = request.Dto.DeliveryAddress;
        entity.Notes = request.Dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        var incomingItemIds = request.Dto.Items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        foreach (var item in entity.Items.Where(x => !incomingItemIds.Contains(x.Id)))
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
        }
        foreach (var itemDto in request.Dto.Items)
        {
            var lineTotal = itemDto.OrderedQuantity * itemDto.UnitPrice;
            var taxAmt = lineTotal * itemDto.TaxPercent / 100;
            var discAmt = lineTotal * itemDto.DiscountPercent / 100;
            var total = lineTotal + taxAmt - discAmt;

            if (itemDto.Id.HasValue)
            {
                var existing = entity.Items.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                if (existing != null)
                {
                    existing.ItemVariantId = itemDto.ItemVariantId;
                    existing.OrderedQuantity = itemDto.OrderedQuantity;
                    existing.UnitPrice = itemDto.UnitPrice;
                    existing.TaxPercent = itemDto.TaxPercent;
                    existing.DiscountPercent = itemDto.DiscountPercent;
                    existing.TotalPrice = total;
                    existing.Notes = itemDto.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                entity.Items.Add(new PurchaseOrderItem
                {
                    ItemVariantId = itemDto.ItemVariantId,
                    OrderedQuantity = itemDto.OrderedQuantity,
                    ReceivedQuantity = 0,
                    UnitPrice = itemDto.UnitPrice,
                    TaxPercent = itemDto.TaxPercent,
                    DiscountPercent = itemDto.DiscountPercent,
                    TotalPrice = total,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var activeItems = entity.Items.Where(x => !x.IsDeleted).ToList();
        entity.SubTotal = activeItems.Sum(x => x.OrderedQuantity * x.UnitPrice);
        entity.TaxAmount = activeItems.Sum(x => x.OrderedQuantity * x.UnitPrice * x.TaxPercent / 100);
        entity.DiscountAmount = activeItems.Sum(x => x.OrderedQuantity * x.UnitPrice * x.DiscountPercent / 100);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount - entity.DiscountAmount;

        var updated = await _repo.UpdateAsync(entity);

        var quotationIds = updated.QuotationId.HasValue ? new List<Guid> { updated.QuotationId.Value } : new List<Guid>();
        var prIds = updated.PRId.HasValue ? new List<Guid> { updated.PRId.Value } : new List<Guid>();
        var approverIds = updated.ApprovedById.HasValue ? new List<Guid> { updated.ApprovedById.Value } : new List<Guid>();
        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var vendor = await _context.Set<Vendor>().FindAsync(new object[] { updated.VendorId }, ct);
        updated.Vendor = vendor!;
        var quotations = await _context.Set<VendorQuotation>().Where(x => quotationIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.QuotationNumber, ct);
        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var approvers = await _context.Set<Employee>().Where(x => approverIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return GetPOsQueryHandler.BuildDto(updated, quotations, prs, approvers, variants);
    }
}

public class DeletePOCommandHandler : IRequestHandler<DeletePOCommand, Unit>
{
    private readonly IPurchaseOrderRepository _repo;
    public DeletePOCommandHandler(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeletePOCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(PurchaseOrder), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}

public class SendPOCommandHandler : IRequestHandler<SendPOCommand, POResponseDto>
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly AppDbContext _context;

    public SendPOCommandHandler(IPurchaseOrderRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<POResponseDto> Handle(SendPOCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (entity.Status != POStatus.Draft)
            throw new AppException("Only Draft purchase orders can be sent.");

        entity.Status = POStatus.Sent;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return await BuildResponseAsync(updated, ct);
    }

    private async Task<POResponseDto> BuildResponseAsync(PurchaseOrder po, CancellationToken ct)
    {
        var variantIds = po.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var vendor = await _context.Set<Vendor>().FindAsync(new object[] { po.VendorId }, ct);
        po.Vendor = vendor!;
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GetPOsQueryHandler.BuildDto(po, new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), variants);
    }
}

public class ConfirmPOCommandHandler : IRequestHandler<ConfirmPOCommand, POResponseDto>
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly AppDbContext _context;

    public ConfirmPOCommandHandler(IPurchaseOrderRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<POResponseDto> Handle(ConfirmPOCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (entity.Status != POStatus.Sent)
            throw new AppException("Only Sent purchase orders can be confirmed.");

        entity.Status = POStatus.Confirmed;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return await BuildResponseAsync(updated, ct);
    }

    private async Task<POResponseDto> BuildResponseAsync(PurchaseOrder po, CancellationToken ct)
    {
        var variantIds = po.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var vendor = await _context.Set<Vendor>().FindAsync(new object[] { po.VendorId }, ct);
        po.Vendor = vendor!;
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GetPOsQueryHandler.BuildDto(po, new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), variants);
    }
}

public class CancelPOCommandHandler : IRequestHandler<CancelPOCommand, POResponseDto>
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly AppDbContext _context;

    public CancelPOCommandHandler(IPurchaseOrderRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<POResponseDto> Handle(CancelPOCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (entity.Status != POStatus.Sent)
            throw new AppException("Only Sent purchase orders can be cancelled.");

        entity.Status = POStatus.Cancelled;
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return await BuildResponseAsync(updated, ct);
    }

    private async Task<POResponseDto> BuildResponseAsync(PurchaseOrder po, CancellationToken ct)
    {
        var variantIds = po.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var vendor = await _context.Set<Vendor>().FindAsync(new object[] { po.VendorId }, ct);
        po.Vendor = vendor!;
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GetPOsQueryHelper.BuildDtoSimple(po, variants);
    }
}

internal static class GetPOsQueryHelper
{
    internal static POResponseDto BuildDtoSimple(PurchaseOrder po, Dictionary<Guid, dynamic> variants) =>
        GetPOsQueryHandler.BuildDto(po, new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), new Dictionary<Guid, string>(), variants);
}
