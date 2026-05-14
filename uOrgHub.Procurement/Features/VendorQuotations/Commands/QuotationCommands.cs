using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.VendorQuotations.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.VendorQuotations.Commands;

public record CreateVendorQuotationCommand(CreateVendorQuotationDto Dto) : ICommand<VendorQuotationResponseDto>;
public record UpdateVendorQuotationCommand(Guid Id, UpdateVendorQuotationDto Dto) : ICommand<VendorQuotationResponseDto>;
public record DeleteVendorQuotationCommand(Guid Id) : ICommand<Unit>;

public class CreateVendorQuotationCommandHandler : IRequestHandler<CreateVendorQuotationCommand, VendorQuotationResponseDto>
{
    private readonly IVendorQuotationRepository _repo;
    private readonly AppDbContext _context;

    public CreateVendorQuotationCommandHandler(IVendorQuotationRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<VendorQuotationResponseDto> Handle(CreateVendorQuotationCommand request, CancellationToken ct)
    {
        var vendor = await _context.Set<Vendor>().Where(x => !x.IsDeleted && x.Id == request.Dto.VendorId).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Vendor), request.Dto.VendorId);

        if (vendor.Status == VendorStatus.Blacklisted)
            throw new AppException("Blacklisted vendors cannot submit quotations.");

        var quotationNumber = await _repo.GenerateQuotationNumberAsync();
        var items = request.Dto.Items.Select(i => new VendorQuotationItem
        {
            RFQItemId = i.RFQItemId,
            ItemVariantId = i.ItemVariantId,
            QuotedQuantity = i.QuotedQuantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.QuotedQuantity * i.UnitPrice,
            Notes = i.Notes,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var entity = new VendorQuotation
        {
            QuotationNumber = quotationNumber,
            RFQId = request.Dto.RFQId,
            VendorId = request.Dto.VendorId,
            QuotationDate = request.Dto.QuotationDate,
            ValidUntil = request.Dto.ValidUntil,
            Status = QuotationStatus.Received,
            TotalAmount = items.Sum(x => x.TotalPrice),
            DeliveryDays = request.Dto.DeliveryDays,
            PaymentTerms = request.Dto.PaymentTerms,
            Notes = request.Dto.Notes,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };
        var created = await _repo.CreateAsync(entity);

        var loaded = await _repo.GetByIdWithItemsAsync(created.Id)
            ?? throw new NotFoundException(nameof(VendorQuotation), created.Id);
        var variantIds = loaded.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GetQuotationsQueryHandler.BuildDto(loaded, variants);
    }
}

public class UpdateVendorQuotationCommandHandler : IRequestHandler<UpdateVendorQuotationCommand, VendorQuotationResponseDto>
{
    private readonly IVendorQuotationRepository _repo;
    private readonly AppDbContext _context;

    public UpdateVendorQuotationCommandHandler(IVendorQuotationRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<VendorQuotationResponseDto> Handle(UpdateVendorQuotationCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(VendorQuotation), request.Id);

        entity.QuotationDate = request.Dto.QuotationDate;
        entity.ValidUntil = request.Dto.ValidUntil;
        entity.Status = request.Dto.Status;
        entity.DeliveryDays = request.Dto.DeliveryDays;
        entity.PaymentTerms = request.Dto.PaymentTerms;
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
            if (itemDto.Id.HasValue)
            {
                var existing = entity.Items.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                if (existing != null)
                {
                    existing.RFQItemId = itemDto.RFQItemId;
                    existing.ItemVariantId = itemDto.ItemVariantId;
                    existing.QuotedQuantity = itemDto.QuotedQuantity;
                    existing.UnitPrice = itemDto.UnitPrice;
                    existing.TotalPrice = itemDto.QuotedQuantity * itemDto.UnitPrice;
                    existing.Notes = itemDto.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                entity.Items.Add(new VendorQuotationItem
                {
                    RFQItemId = itemDto.RFQItemId,
                    ItemVariantId = itemDto.ItemVariantId,
                    QuotedQuantity = itemDto.QuotedQuantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.QuotedQuantity * itemDto.UnitPrice,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        entity.TotalAmount = entity.Items.Where(x => !x.IsDeleted).Sum(x => x.TotalPrice);
        var updated = await _repo.UpdateAsync(entity);

        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));
        return GetQuotationsQueryHandler.BuildDto(updated, variants);
    }
}

public class DeleteVendorQuotationCommandHandler : IRequestHandler<DeleteVendorQuotationCommand, Unit>
{
    private readonly IVendorQuotationRepository _repo;
    public DeleteVendorQuotationCommandHandler(IVendorQuotationRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteVendorQuotationCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(VendorQuotation), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
