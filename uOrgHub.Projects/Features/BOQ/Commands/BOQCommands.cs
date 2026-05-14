using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.BOQ.Commands;

public record CreateBOQCommand(CreateBOQDto Dto) : ICommand<BOQResponseDto>;
public record UpdateBOQCommand(Guid Id, UpdateBOQDto Dto) : ICommand<BOQResponseDto>;
public record DeleteBOQCommand(Guid Id) : ICommand<Unit>;
public record ApproveBOQCommand(Guid Id, ApproveBOQDto Dto) : ICommand<BOQResponseDto>;
public record AddBOQItemCommand(Guid BOQId, CreateBOQItemDto Dto) : ICommand<BOQResponseDto>;
public record UpdateBOQItemCommand(Guid BOQId, Guid ItemId, UpdateBOQItemDto Dto) : ICommand<BOQResponseDto>;
public record DeleteBOQItemCommand(Guid BOQId, Guid ItemId) : ICommand<BOQResponseDto>;

public class CreateBOQCommandHandler : IRequestHandler<CreateBOQCommand, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public CreateBOQCommandHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(CreateBOQCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var count = await _context.Set<BillOfQuantity>().CountAsync(ct);
        var boqNumber = $"BOQ-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new BillOfQuantity
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            BOQNumber = boqNumber,
            Title = dto.Title,
            Description = dto.Description,
            Status = BOQStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            var estimatedAmount = itemDto.EstimatedQuantity * itemDto.UnitRate;
            entity.Items.Add(new BOQItem
            {
                ItemVariantId = itemDto.ItemVariantId,
                ItemDescription = itemDto.ItemDescription,
                Specification = itemDto.Specification,
                UnitOfMeasure = itemDto.UnitOfMeasure,
                EstimatedQuantity = itemDto.EstimatedQuantity,
                UnitRate = itemDto.UnitRate,
                EstimatedAmount = estimatedAmount,
                Sequence = itemDto.Sequence,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.TotalEstimatedCost = entity.Items.Sum(i => i.EstimatedAmount);
        _context.Set<BillOfQuantity>().Add(entity);
        await _context.SaveChangesAsync(ct);

        return BOQMapper.ToDto(entity);
    }
}

public class UpdateBOQCommandHandler : IRequestHandler<UpdateBOQCommand, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateBOQCommandHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(UpdateBOQCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.Id);

        if (entity.Status == BOQStatus.Approved)
            throw new AppException("Approved BOQ cannot be modified.");

        entity.WBSId = request.Dto.WBSId;
        entity.Title = request.Dto.Title;
        entity.Description = request.Dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BOQMapper.ToDto(entity);
    }
}

public class DeleteBOQCommandHandler : IRequestHandler<DeleteBOQCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteBOQCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteBOQCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<BillOfQuantity>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class ApproveBOQCommandHandler : IRequestHandler<ApproveBOQCommand, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveBOQCommandHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(ApproveBOQCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.Id);

        if (entity.Status != BOQStatus.Draft)
            throw new AppException("Only Draft BOQs can be approved.");

        entity.Status = BOQStatus.Approved;
        entity.ApprovedById = request.Dto.ApprovedById;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BOQMapper.ToDto(entity);
    }
}

public class AddBOQItemCommandHandler : IRequestHandler<AddBOQItemCommand, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public AddBOQItemCommandHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(AddBOQItemCommand request, CancellationToken ct)
    {
        var boq = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.BOQId, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.BOQId);

        if (boq.Status == BOQStatus.Approved)
            throw new AppException("Cannot add items to an approved BOQ.");

        var dto = request.Dto;
        var estimatedAmount = dto.EstimatedQuantity * dto.UnitRate;
        var item = new BOQItem
        {
            BOQId = request.BOQId,
            ItemVariantId = dto.ItemVariantId,
            ItemDescription = dto.ItemDescription,
            Specification = dto.Specification,
            UnitOfMeasure = dto.UnitOfMeasure,
            EstimatedQuantity = dto.EstimatedQuantity,
            UnitRate = dto.UnitRate,
            EstimatedAmount = estimatedAmount,
            Sequence = dto.Sequence,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<BOQItem>().Add(item);
        boq.TotalEstimatedCost = boq.Items.Sum(i => i.EstimatedAmount) + estimatedAmount;
        boq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        // Reload to get updated items
        var updated = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstAsync(x => x.Id == request.BOQId, ct);
        return BOQMapper.ToDto(updated);
    }
}

public class UpdateBOQItemCommandHandler : IRequestHandler<UpdateBOQItemCommand, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateBOQItemCommandHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(UpdateBOQItemCommand request, CancellationToken ct)
    {
        var boq = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.BOQId, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.BOQId);

        if (boq.Status == BOQStatus.Approved)
            throw new AppException("Cannot modify items of an approved BOQ.");

        var item = boq.Items.FirstOrDefault(x => !x.IsDeleted && x.Id == request.ItemId)
            ?? throw new NotFoundException(nameof(BOQItem), request.ItemId);

        var dto = request.Dto;
        item.ItemVariantId = dto.ItemVariantId;
        item.ItemDescription = dto.ItemDescription;
        item.Specification = dto.Specification;
        item.UnitOfMeasure = dto.UnitOfMeasure;
        item.EstimatedQuantity = dto.EstimatedQuantity;
        item.UnitRate = dto.UnitRate;
        item.EstimatedAmount = dto.EstimatedQuantity * dto.UnitRate;
        item.ActualQuantity = dto.ActualQuantity;
        item.ActualAmount = dto.ActualAmount;
        item.Sequence = dto.Sequence;
        item.UpdatedAt = DateTime.UtcNow;

        boq.TotalEstimatedCost = boq.Items.Where(i => !i.IsDeleted).Sum(i => i.EstimatedAmount);
        boq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BOQMapper.ToDto(boq);
    }
}

public class DeleteBOQItemCommandHandler : IRequestHandler<DeleteBOQItemCommand, BOQResponseDto>
{
    private readonly AppDbContext _context;
    public DeleteBOQItemCommandHandler(AppDbContext context) => _context = context;

    public async Task<BOQResponseDto> Handle(DeleteBOQItemCommand request, CancellationToken ct)
    {
        var boq = await _context.Set<BillOfQuantity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.BOQId, ct)
            ?? throw new NotFoundException(nameof(BillOfQuantity), request.BOQId);

        if (boq.Status == BOQStatus.Approved)
            throw new AppException("Cannot delete items from an approved BOQ.");

        var item = boq.Items.FirstOrDefault(x => !x.IsDeleted && x.Id == request.ItemId)
            ?? throw new NotFoundException(nameof(BOQItem), request.ItemId);

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        boq.TotalEstimatedCost = boq.Items.Where(i => !i.IsDeleted).Sum(i => i.EstimatedAmount);
        boq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BOQMapper.ToDto(boq);
    }
}

public static class BOQMapper
{
    public static BOQResponseDto ToDto(BillOfQuantity e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        BOQNumber = e.BOQNumber,
        Title = e.Title,
        Description = e.Description,
        Status = e.Status,
        TotalEstimatedCost = e.TotalEstimatedCost,
        ApprovedById = e.ApprovedById,
        ApprovedAt = e.ApprovedAt,
        CreatedAt = e.CreatedAt,
        Items = e.Items
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.Sequence)
            .Select(ToItemDto)
            .ToList()
    };

    public static BOQItemResponseDto ToItemDto(BOQItem i) => new()
    {
        Id = i.Id,
        BOQId = i.BOQId,
        ItemVariantId = i.ItemVariantId,
        ItemDescription = i.ItemDescription,
        Specification = i.Specification,
        UnitOfMeasure = i.UnitOfMeasure,
        EstimatedQuantity = i.EstimatedQuantity,
        UnitRate = i.UnitRate,
        EstimatedAmount = i.EstimatedAmount,
        ActualQuantity = i.ActualQuantity,
        ActualAmount = i.ActualAmount,
        Sequence = i.Sequence
    };
}
