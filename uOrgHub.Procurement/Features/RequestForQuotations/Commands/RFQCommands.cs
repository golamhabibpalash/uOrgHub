using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.RequestForQuotations.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Procurement.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.RequestForQuotations.Commands;

public record CreateRFQCommand(CreateRFQDto Dto) : ICommand<RFQResponseDto>;
public record UpdateRFQCommand(Guid Id, UpdateRFQDto Dto) : ICommand<RFQResponseDto>;
public record DeleteRFQCommand(Guid Id) : ICommand<Unit>;

public class CreateRFQCommandHandler : IRequestHandler<CreateRFQCommand, RFQResponseDto>
{
    private readonly IRequestForQuotationRepository _repo;
    private readonly AppDbContext _context;

    public CreateRFQCommandHandler(IRequestForQuotationRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<RFQResponseDto> Handle(CreateRFQCommand request, CancellationToken ct)
    {
        var rfqNumber = await _repo.GenerateRFQNumberAsync();
        var entity = new RequestForQuotation
        {
            RFQNumber = rfqNumber,
            RFQDate = request.Dto.RFQDate,
            ClosingDate = request.Dto.ClosingDate,
            PRId = request.Dto.PRId,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            Notes = request.Dto.Notes,
            Status = RFQStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Items = request.Dto.Items.Select(i => new RFQItem
            {
                ItemVariantId = i.ItemVariantId,
                RequestedQuantity = i.RequestedQuantity,
                Notes = i.Notes,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };
        var created = await _repo.CreateAsync(entity);
        return await BuildResponseAsync(created, ct);
    }

    private async Task<RFQResponseDto> BuildResponseAsync(RequestForQuotation rfq, CancellationToken ct)
    {
        var prIds = rfq.PRId.HasValue ? new List<Guid> { rfq.PRId.Value } : new List<Guid>();
        var variantIds = rfq.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName));
        return GetRFQsQueryHandler.BuildDto(rfq, prs, variants);
    }
}

public class UpdateRFQCommandHandler : IRequestHandler<UpdateRFQCommand, RFQResponseDto>
{
    private readonly IRequestForQuotationRepository _repo;
    private readonly AppDbContext _context;

    public UpdateRFQCommandHandler(IRequestForQuotationRepository repo, AppDbContext context)
    { _repo = repo; _context = context; }

    public async Task<RFQResponseDto> Handle(UpdateRFQCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithItemsAsync(request.Id)
            ?? throw new NotFoundException(nameof(RequestForQuotation), request.Id);

        entity.RFQDate = request.Dto.RFQDate;
        entity.ClosingDate = request.Dto.ClosingDate;
        entity.PRId = request.Dto.PRId;
        entity.Title = request.Dto.Title;
        entity.Description = request.Dto.Description;
        entity.Status = request.Dto.Status;
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
                    existing.ItemVariantId = itemDto.ItemVariantId;
                    existing.RequestedQuantity = itemDto.RequestedQuantity;
                    existing.Notes = itemDto.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                entity.Items.Add(new RFQItem
                {
                    ItemVariantId = itemDto.ItemVariantId,
                    RequestedQuantity = itemDto.RequestedQuantity,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var updated = await _repo.UpdateAsync(entity);

        var prIds = updated.PRId.HasValue ? new List<Guid> { updated.PRId.Value } : new List<Guid>();
        var variantIds = updated.Items.Where(x => !x.IsDeleted).Select(x => x.ItemVariantId).Distinct().ToList();
        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName));
        return GetRFQsQueryHandler.BuildDto(updated, prs, variants);
    }
}

public class DeleteRFQCommandHandler : IRequestHandler<DeleteRFQCommand, Unit>
{
    private readonly IRequestForQuotationRepository _repo;
    public DeleteRFQCommandHandler(IRequestForQuotationRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteRFQCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(RequestForQuotation), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
