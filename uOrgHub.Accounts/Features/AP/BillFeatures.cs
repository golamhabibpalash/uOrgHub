using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.AP;

public record GetBillsQuery(PaginationRequest Request, Guid? VendorId = null, BillStatus? Status = null) : IQuery<PagedResult<BillResponseDto>>;
public record GetBillByIdQuery(Guid Id) : IQuery<BillResponseDto>;
public record CreateBillCommand(CreateBillDto Dto) : ICommand<BillResponseDto>;
public record UpdateBillCommand(Guid Id, UpdateBillDto Dto) : ICommand<BillResponseDto>;
public record ApproveBillCommand(Guid Id) : ICommand<BillResponseDto>;
public record VoidBillCommand(Guid Id) : ICommand<BillResponseDto>;

public class GetBillsQueryHandler : IRequestHandler<GetBillsQuery, PagedResult<BillResponseDto>>
{
    private readonly AppDbContext _context;
    public GetBillsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<BillResponseDto>> Handle(GetBillsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Bill>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted);

        if (request.VendorId.HasValue)
            query = query.Where(x => x.VendorId == request.VendorId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.BillNumber.Contains(request.Request.Search)
                || x.Vendor.Name.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.BillDate)
            : query.OrderBy(x => x.BillDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BillResponseDto>
        {
            Items = items.Select(BillMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, BillResponseDto>
{
    private readonly AppDbContext _context;
    public GetBillByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<BillResponseDto> Handle(GetBillByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Bill>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Bill), request.Id);

        return BillMappingHelper.ToDto(e);
    }
}

public class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, BillResponseDto>
{
    private readonly AppDbContext _context;
    public CreateBillCommandHandler(AppDbContext context) => _context = context;

    public async Task<BillResponseDto> Handle(CreateBillCommand request, CancellationToken ct)
    {
        if (await _context.Set<Models.Entities.Bill>().AnyAsync(x => x.BillNumber == request.Dto.BillNumber && !x.IsDeleted, ct))
            throw new AppException($"Bill number '{request.Dto.BillNumber}' already exists.");

        var entity = new Models.Entities.Bill
        {
            BillNumber = request.Dto.BillNumber,
            VendorBillNumber = request.Dto.VendorBillNumber,
            VendorId = request.Dto.VendorId,
            FiscalYearId = request.Dto.FiscalYearId,
            BillDate = request.Dto.BillDate,
            DueDate = request.Dto.DueDate,
            Notes = request.Dto.Notes,
            CostCenterId = request.Dto.CostCenterId,
            Status = BillStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineDto in request.Dto.Lines)
        {
            var taxRate = lineDto.TaxRateId.HasValue
                ? await _context.Set<Models.Entities.TaxRate>().FindAsync(new object[] { lineDto.TaxRateId.Value }, ct)
                : null;
            var lineTotal = (lineDto.Quantity * lineDto.UnitPrice) * (1 - lineDto.DiscountPercent / 100);
            var taxAmount = taxRate != null ? lineTotal * taxRate.Rate / 100 : 0;

            entity.Lines.Add(new Models.Entities.BillLine
            {
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                TaxAmount = Math.Round(taxAmount, 2),
                LineTotal = Math.Round(lineTotal + taxAmount, 2),
                LineOrder = lineDto.LineOrder,
                TaxRateId = lineDto.TaxRateId,
                ExpenseAccountId = lineDto.ExpenseAccountId,
                CostCenterId = lineDto.CostCenterId,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.SubTotal = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100));
        entity.TaxAmount = entity.Lines.Sum(l => l.TaxAmount);
        entity.DiscountAmount = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;

        _context.Set<Models.Entities.Bill>().Add(entity);
        await _context.SaveChangesAsync(ct);

        await _context.Entry(entity).Reference(x => x.Vendor).LoadAsync(ct);
        return BillMappingHelper.ToDto(entity);
    }
}

public class UpdateBillCommandHandler : IRequestHandler<UpdateBillCommand, BillResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateBillCommandHandler(AppDbContext context) => _context = context;

    public async Task<BillResponseDto> Handle(UpdateBillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Bill>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Bill), request.Id);

        if (entity.Status != BillStatus.Draft)
            throw new AppException("Only draft bills can be updated.");

        entity.VendorBillNumber = request.Dto.VendorBillNumber;
        entity.BillDate = request.Dto.BillDate;
        entity.DueDate = request.Dto.DueDate;
        entity.Notes = request.Dto.Notes;
        entity.CostCenterId = request.Dto.CostCenterId;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<Models.Entities.BillLine>().RemoveRange(entity.Lines);
        entity.Lines.Clear();

        foreach (var lineDto in request.Dto.Lines)
        {
            var taxRate = lineDto.TaxRateId.HasValue
                ? await _context.Set<Models.Entities.TaxRate>().FindAsync(new object[] { lineDto.TaxRateId.Value }, ct)
                : null;
            var lineTotal = (lineDto.Quantity * lineDto.UnitPrice) * (1 - lineDto.DiscountPercent / 100);
            var taxAmount = taxRate != null ? lineTotal * taxRate.Rate / 100 : 0;

            entity.Lines.Add(new Models.Entities.BillLine
            {
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                TaxAmount = Math.Round(taxAmount, 2),
                LineTotal = Math.Round(lineTotal + taxAmount, 2),
                LineOrder = lineDto.LineOrder,
                TaxRateId = lineDto.TaxRateId,
                ExpenseAccountId = lineDto.ExpenseAccountId,
                CostCenterId = lineDto.CostCenterId,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.SubTotal = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100));
        entity.TaxAmount = entity.Lines.Sum(l => l.TaxAmount);
        entity.DiscountAmount = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;

        await _context.SaveChangesAsync(ct);
        return BillMappingHelper.ToDto(entity);
    }
}

public class ApproveBillCommandHandler : IRequestHandler<ApproveBillCommand, BillResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveBillCommandHandler(AppDbContext context) => _context = context;

    public async Task<BillResponseDto> Handle(ApproveBillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Bill>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Bill), request.Id);

        if (entity.Status != BillStatus.Draft)
            throw new AppException("Only draft bills can be approved.");

        entity.Status = BillStatus.Received;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BillMappingHelper.ToDto(entity);
    }
}

public class VoidBillCommandHandler : IRequestHandler<VoidBillCommand, BillResponseDto>
{
    private readonly AppDbContext _context;
    public VoidBillCommandHandler(AppDbContext context) => _context = context;

    public async Task<BillResponseDto> Handle(VoidBillCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Bill>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Bill), request.Id);

        if (entity.Status == BillStatus.Paid || entity.Status == BillStatus.Void)
            throw new AppException($"Cannot void a bill with status '{entity.Status}'.");

        entity.Status = BillStatus.Void;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BillMappingHelper.ToDto(entity);
    }
}

file static class BillMappingHelper
{
    public static BillResponseDto ToDto(Models.Entities.Bill e) => new()
    {
        Id = e.Id,
        BillNumber = e.BillNumber,
        VendorBillNumber = e.VendorBillNumber,
        VendorId = e.VendorId,
        VendorName = e.Vendor?.Name ?? string.Empty,
        FiscalYearId = e.FiscalYearId,
        BillDate = e.BillDate,
        DueDate = e.DueDate,
        Status = e.Status,
        SubTotal = e.SubTotal,
        TaxAmount = e.TaxAmount,
        DiscountAmount = e.DiscountAmount,
        TotalAmount = e.TotalAmount,
        PaidAmount = e.PaidAmount,
        Notes = e.Notes,
        CostCenterId = e.CostCenterId,
        JournalEntryId = e.JournalEntryId,
        Lines = e.Lines.Select(l => new BillLineResponseDto
        {
            Id = l.Id,
            Description = l.Description,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            DiscountPercent = l.DiscountPercent,
            TaxAmount = l.TaxAmount,
            LineTotal = l.LineTotal,
            LineOrder = l.LineOrder,
            TaxRateId = l.TaxRateId,
            ExpenseAccountId = l.ExpenseAccountId,
            CostCenterId = l.CostCenterId
        }).ToList()
    };
}
