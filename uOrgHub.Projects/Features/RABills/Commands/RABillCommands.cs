using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.RABills.Commands;

public record CreateRABillCommand(CreateRABillDto Dto) : ICommand<RABillResponseDto>;
public record UpdateRABillCommand(Guid Id, UpdateRABillDto Dto) : ICommand<RABillResponseDto>;
public record SubmitRABillCommand(Guid Id) : ICommand<RABillResponseDto>;
public record CertifyRABillCommand(Guid Id, CertifyRABillDto Dto) : ICommand<RABillResponseDto>;
public record MarkRABillPaidCommand(Guid Id) : ICommand<RABillResponseDto>;
public record DeleteRABillCommand(Guid Id) : ICommand<Unit>;

public class CreateRABillCommandHandler : IRequestHandler<CreateRABillCommand, RABillResponseDto>
{
    private readonly AppDbContext _context;
    public CreateRABillCommandHandler(AppDbContext context) => _context = context;

    public async Task<RABillResponseDto> Handle(CreateRABillCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        if (dto.PeriodTo <= dto.PeriodFrom)
            throw new AppException("Period end date must be after period start date.");

        var sequence = await _context.Set<RABill>()
            .CountAsync(x => x.ProjectId == dto.ProjectId, ct) + 1;

        var billNumber = $"RAB-{dto.ProjectId.ToString()[..4].ToUpper()}-{sequence:D3}";

        var previousTotal = await _context.Set<RABill>()
            .Where(x => x.ProjectId == dto.ProjectId && !x.IsDeleted
                && (x.Status == RABillStatus.Certified || x.Status == RABillStatus.Paid))
            .SumAsync(x => x.NetAmount, ct);

        var entity = new RABill
        {
            ProjectId = dto.ProjectId,
            BillNumber = billNumber,
            Title = dto.Title,
            BillDate = dto.BillDate,
            PeriodFrom = dto.PeriodFrom,
            PeriodTo = dto.PeriodTo,
            BillSequence = sequence,
            SubmittedById = dto.SubmittedById,
            RetentionPercent = dto.RetentionPercent,
            PreviousBilledAmount = previousTotal,
            Status = RABillStatus.Draft,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            var totalQty = itemDto.PreviousQuantity + itemDto.CurrentQuantity;
            var amount = itemDto.CurrentQuantity * itemDto.Rate;
            entity.Items.Add(new RABillItem
            {
                BOQItemId = itemDto.BOQItemId,
                Description = itemDto.Description,
                UnitOfMeasure = itemDto.UnitOfMeasure,
                PreviousQuantity = itemDto.PreviousQuantity,
                CurrentQuantity = itemDto.CurrentQuantity,
                TotalQuantity = totalQty,
                Rate = itemDto.Rate,
                Amount = amount,
                Sequence = itemDto.Sequence,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.GrossAmount = entity.Items.Sum(i => i.Amount);
        entity.RetentionAmount = entity.GrossAmount * (dto.RetentionPercent / 100);
        entity.NetAmount = entity.GrossAmount - entity.RetentionAmount;
        entity.CumulativeBilledAmount = previousTotal + entity.NetAmount;

        _context.Set<RABill>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Set<RABill>()
            .Include(x => x.Items)
            .FirstAsync(x => x.Id == entity.Id, ct);
        return RABillMapper.ToDto(created);
    }
}

public class UpdateRABillCommandHandler : IRequestHandler<UpdateRABillCommand, RABillResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateRABillCommandHandler(AppDbContext context) => _context = context;

    public async Task<RABillResponseDto> Handle(UpdateRABillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<RABill>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RABill), request.Id);

        if (entity.Status != RABillStatus.Draft)
            throw new AppException("Only draft RA bills can be modified.");

        if (request.Dto.PeriodTo <= request.Dto.PeriodFrom)
            throw new AppException("Period end date must be after period start date.");

        var dto = request.Dto;
        entity.Title = dto.Title;
        entity.BillDate = dto.BillDate;
        entity.PeriodFrom = dto.PeriodFrom;
        entity.PeriodTo = dto.PeriodTo;
        entity.RetentionPercent = dto.RetentionPercent;
        entity.RetentionAmount = entity.GrossAmount * (dto.RetentionPercent / 100);
        entity.NetAmount = entity.GrossAmount - entity.RetentionAmount;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RABillMapper.ToDto(entity);
    }
}

public class SubmitRABillCommandHandler : IRequestHandler<SubmitRABillCommand, RABillResponseDto>
{
    private readonly AppDbContext _context;
    public SubmitRABillCommandHandler(AppDbContext context) => _context = context;

    public async Task<RABillResponseDto> Handle(SubmitRABillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<RABill>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RABill), request.Id);

        if (entity.Status != RABillStatus.Draft)
            throw new AppException("Only draft RA bills can be submitted.");

        if (!entity.Items.Any(i => !i.IsDeleted))
            throw new AppException("RA bill must have at least one item before submission.");

        entity.Status = RABillStatus.Submitted;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RABillMapper.ToDto(entity);
    }
}

public class CertifyRABillCommandHandler : IRequestHandler<CertifyRABillCommand, RABillResponseDto>
{
    private readonly AppDbContext _context;
    public CertifyRABillCommandHandler(AppDbContext context) => _context = context;

    public async Task<RABillResponseDto> Handle(CertifyRABillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<RABill>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RABill), request.Id);

        if (entity.Status != RABillStatus.Submitted && entity.Status != RABillStatus.UnderReview)
            throw new AppException("Only submitted or under-review RA bills can be certified.");

        var dto = request.Dto;
        entity.GrossAmount = dto.GrossAmount;
        entity.DeductionAmount = dto.DeductionAmount;
        entity.RetentionAmount = (dto.GrossAmount - dto.DeductionAmount) * (entity.RetentionPercent / 100);
        entity.NetAmount = dto.GrossAmount - dto.DeductionAmount - entity.RetentionAmount;
        entity.CertifiedById = dto.CertifiedById;
        entity.CertifiedDate = dto.CertifiedDate;
        entity.Status = RABillStatus.Certified;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RABillMapper.ToDto(entity);
    }
}

public class MarkRABillPaidCommandHandler : IRequestHandler<MarkRABillPaidCommand, RABillResponseDto>
{
    private readonly AppDbContext _context;
    public MarkRABillPaidCommandHandler(AppDbContext context) => _context = context;

    public async Task<RABillResponseDto> Handle(MarkRABillPaidCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<RABill>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RABill), request.Id);

        if (entity.Status != RABillStatus.Certified)
            throw new AppException("Only certified RA bills can be marked as paid.");

        entity.Status = RABillStatus.Paid;
        entity.PaidDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RABillMapper.ToDto(entity);
    }
}

public class DeleteRABillCommandHandler : IRequestHandler<DeleteRABillCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteRABillCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteRABillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<RABill>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RABill), request.Id);

        if (entity.Status != RABillStatus.Draft)
            throw new AppException("Only draft RA bills can be deleted.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class RABillMapper
{
    public static RABillResponseDto ToDto(RABill e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        BillNumber = e.BillNumber,
        Title = e.Title,
        BillDate = e.BillDate,
        PeriodFrom = e.PeriodFrom,
        PeriodTo = e.PeriodTo,
        BillSequence = e.BillSequence,
        SubmittedById = e.SubmittedById,
        CertifiedById = e.CertifiedById,
        CertifiedDate = e.CertifiedDate,
        PaidDate = e.PaidDate,
        GrossAmount = e.GrossAmount,
        DeductionAmount = e.DeductionAmount,
        RetentionPercent = e.RetentionPercent,
        RetentionAmount = e.RetentionAmount,
        NetAmount = e.NetAmount,
        PreviousBilledAmount = e.PreviousBilledAmount,
        CumulativeBilledAmount = e.CumulativeBilledAmount,
        Status = e.Status,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        Items = e.Items
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.Sequence)
            .Select(i => new RABillItemResponseDto
            {
                Id = i.Id,
                RABillId = i.RABillId,
                BOQItemId = i.BOQItemId,
                Description = i.Description,
                UnitOfMeasure = i.UnitOfMeasure,
                PreviousQuantity = i.PreviousQuantity,
                CurrentQuantity = i.CurrentQuantity,
                TotalQuantity = i.TotalQuantity,
                Rate = i.Rate,
                Amount = i.Amount,
                Sequence = i.Sequence
            }).ToList()
    };
}
