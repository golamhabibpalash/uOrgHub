using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.AR;

public record GetInvoicesQuery(PaginationRequest Request, Guid? CustomerId = null, InvoiceStatus? Status = null) : IQuery<PagedResult<InvoiceResponseDto>>;
public record GetInvoiceByIdQuery(Guid Id) : IQuery<InvoiceResponseDto>;
public record CreateInvoiceCommand(CreateInvoiceDto Dto) : ICommand<InvoiceResponseDto>;
public record UpdateInvoiceCommand(Guid Id, UpdateInvoiceDto Dto) : ICommand<InvoiceResponseDto>;
public record PostInvoiceCommand(Guid Id) : ICommand<InvoiceResponseDto>;
public record VoidInvoiceCommand(Guid Id) : ICommand<InvoiceResponseDto>;
public record GetAllInvoicesForExportQuery : IQuery<List<InvoiceResponseDto>>;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceResponseDto>>
{
    private readonly AppDbContext _context;
    public GetInvoicesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<InvoiceResponseDto>> Handle(GetInvoicesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Invoice>()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted);

        if (request.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.InvoiceNumber, x => x.Customer.Name);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.InvoiceDate)
            : query.OrderBy(x => x.InvoiceDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<InvoiceResponseDto>
        {
            Items = items.Select(InvoiceMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceResponseDto>
{
    private readonly AppDbContext _context;
    public GetInvoiceByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<InvoiceResponseDto> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Invoice>()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Invoice), request.Id);

        return InvoiceMappingHelper.ToDto(e);
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IDocumentNumberingService _numbering;

    public CreateInvoiceCommandHandler(AppDbContext context, IDocumentNumberingService numbering)
    {
        _context = context;
        _numbering = numbering;
    }

    public async Task<InvoiceResponseDto> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        var invoiceNumber = request.Dto.InvoiceNumber;
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            invoiceNumber = await _numbering.GenerateNextAsync("Invoice", "INV");

        if (await _context.Set<Models.Entities.Invoice>().AnyAsync(x => x.InvoiceNumber == invoiceNumber && !x.IsDeleted, ct))
            throw new AppException($"Invoice number '{invoiceNumber}' already exists.");

        var entity = new Models.Entities.Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = request.Dto.CustomerId,
            FiscalYearId = request.Dto.FiscalYearId,
            InvoiceDate = request.Dto.InvoiceDate,
            DueDate = request.Dto.DueDate,
            Notes = request.Dto.Notes,
            CostCenterId = request.Dto.CostCenterId,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineDto in request.Dto.Lines)
        {
            var taxRate = lineDto.TaxRateId.HasValue
                ? await _context.Set<Models.Entities.TaxRate>().FindAsync(new object[] { lineDto.TaxRateId.Value }, ct)
                : null;
            var lineTotal = (lineDto.Quantity * lineDto.UnitPrice) * (1 - lineDto.DiscountPercent / 100);
            var taxAmount = taxRate != null ? lineTotal * taxRate.Rate / 100 : 0;

            entity.Lines.Add(new Models.Entities.InvoiceLine
            {
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                TaxAmount = Math.Round(taxAmount, 2),
                LineTotal = Math.Round(lineTotal + taxAmount, 2),
                LineOrder = lineDto.LineOrder,
                TaxRateId = lineDto.TaxRateId,
                RevenueAccountId = lineDto.RevenueAccountId,
                CostCenterId = lineDto.CostCenterId,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.SubTotal = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100));
        entity.TaxAmount = entity.Lines.Sum(l => l.TaxAmount);
        entity.DiscountAmount = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;

        _context.Set<Models.Entities.Invoice>().Add(entity);
        await _context.SaveChangesAsync(ct);

        await _context.Entry(entity).Reference(x => x.Customer).LoadAsync(ct);
        return InvoiceMappingHelper.ToDto(entity);
    }
}

public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, InvoiceResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateInvoiceCommandHandler(AppDbContext context) => _context = context;

    public async Task<InvoiceResponseDto> Handle(UpdateInvoiceCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Invoice>()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Invoice), request.Id);

        if (entity.Status != InvoiceStatus.Draft)
            throw new AppException("Only draft invoices can be updated.");

        entity.InvoiceDate = request.Dto.InvoiceDate;
        entity.DueDate = request.Dto.DueDate;
        entity.Notes = request.Dto.Notes;
        entity.CostCenterId = request.Dto.CostCenterId;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<Models.Entities.InvoiceLine>().RemoveRange(entity.Lines);
        entity.Lines.Clear();

        foreach (var lineDto in request.Dto.Lines)
        {
            var taxRate = lineDto.TaxRateId.HasValue
                ? await _context.Set<Models.Entities.TaxRate>().FindAsync(new object[] { lineDto.TaxRateId.Value }, ct)
                : null;
            var lineTotal = (lineDto.Quantity * lineDto.UnitPrice) * (1 - lineDto.DiscountPercent / 100);
            var taxAmount = taxRate != null ? lineTotal * taxRate.Rate / 100 : 0;

            entity.Lines.Add(new Models.Entities.InvoiceLine
            {
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                TaxAmount = Math.Round(taxAmount, 2),
                LineTotal = Math.Round(lineTotal + taxAmount, 2),
                LineOrder = lineDto.LineOrder,
                TaxRateId = lineDto.TaxRateId,
                RevenueAccountId = lineDto.RevenueAccountId,
                CostCenterId = lineDto.CostCenterId,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.SubTotal = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100));
        entity.TaxAmount = entity.Lines.Sum(l => l.TaxAmount);
        entity.DiscountAmount = entity.Lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;

        await _context.SaveChangesAsync(ct);
        return InvoiceMappingHelper.ToDto(entity);
    }
}

public class PostInvoiceCommandHandler : IRequestHandler<PostInvoiceCommand, InvoiceResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;
    private readonly IJournalEntryRepository _jeRepository;

    public PostInvoiceCommandHandler(AppDbContext context, IJournalEntryService jeService, IJournalEntryRepository jeRepository)
    {
        _context = context;
        _jeService = jeService;
        _jeRepository = jeRepository;
    }

    public async Task<InvoiceResponseDto> Handle(PostInvoiceCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Invoice>()
            .Include(x => x.Customer)
            .Include(x => x.Lines).ThenInclude(l => l.TaxRate)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Invoice), request.Id);

        if (entity.Status != InvoiceStatus.Draft)
            throw new AppException("Only draft invoices can be posted.");

        var je = await BuildAndSaveInvoiceJeAsync(entity, ct);
        await _jeService.PostAsync(je.Id, "System");

        entity.JournalEntryId = je.Id;
        entity.Status = InvoiceStatus.Sent;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return InvoiceMappingHelper.ToDto(entity);
    }

    private async Task<Models.Entities.JournalEntry> BuildAndSaveInvoiceJeAsync(Models.Entities.Invoice invoice, CancellationToken ct)
    {
        var entryNumber = await _jeRepository.GenerateEntryNumberAsync();
        var je = new Models.Entities.JournalEntry
        {
            EntryNumber = entryNumber,
            EntryDate = invoice.InvoiceDate,
            Description = $"Invoice {invoice.InvoiceNumber}",
            ReferenceNumber = invoice.InvoiceNumber,
            Status = JournalEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        int order = 1;
        foreach (var line in invoice.Lines)
        {
            if (line.TaxAmount > 0 && line.TaxRate?.TaxAccountId.HasValue == true)
            {
                je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = line.RevenueAccountId, CreditAmount = line.LineTotal - line.TaxAmount, Description = line.Description, LineOrder = order++, CostCenterId = line.CostCenterId, CreatedAt = DateTime.UtcNow });
                je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = line.TaxRate.TaxAccountId!.Value, CreditAmount = line.TaxAmount, Description = $"Tax - {line.Description}", LineOrder = order++, CostCenterId = line.CostCenterId, CreatedAt = DateTime.UtcNow });
            }
            else
            {
                je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = line.RevenueAccountId, CreditAmount = line.LineTotal, Description = line.Description, LineOrder = order++, CostCenterId = line.CostCenterId, CreatedAt = DateTime.UtcNow });
            }
        }
        je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = invoice.Customer.ReceivableAccountId, DebitAmount = invoice.TotalAmount, Description = $"AR - {invoice.InvoiceNumber}", LineOrder = order, CreatedAt = DateTime.UtcNow });

        je.TotalDebit = je.Lines.Sum(l => l.DebitAmount);
        je.TotalCredit = je.Lines.Sum(l => l.CreditAmount);

        _context.Set<Models.Entities.JournalEntry>().Add(je);
        await _context.SaveChangesAsync(ct);
        return je;
    }
}

public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand, InvoiceResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;

    public VoidInvoiceCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<InvoiceResponseDto> Handle(VoidInvoiceCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Invoice>()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Invoice), request.Id);

        if (entity.Status == InvoiceStatus.Paid || entity.Status == InvoiceStatus.Void)
            throw new AppException($"Cannot void an invoice with status '{entity.Status}'.");

        if (entity.PaidAmount > 0)
            throw new AppException("Cannot void an invoice with existing payments. Please void the payments first.");

        if (entity.JournalEntryId.HasValue)
            await _jeService.CancelAsync(entity.JournalEntryId.Value);

        entity.Status = InvoiceStatus.Void;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return InvoiceMappingHelper.ToDto(entity);
    }
}

public class GetAllInvoicesForExportQueryHandler : IRequestHandler<GetAllInvoicesForExportQuery, List<InvoiceResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllInvoicesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<InvoiceResponseDto>> Handle(GetAllInvoicesForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.Invoice>()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.InvoiceDate)
            .ToListAsync(ct);
        return items.Select(InvoiceMappingHelper.ToDto).ToList();
    }
}

file static class InvoiceMappingHelper
{
    public static InvoiceResponseDto ToDto(Models.Entities.Invoice e) => new()
    {
        Id = e.Id,
        InvoiceNumber = e.InvoiceNumber,
        CustomerId = e.CustomerId,
        CustomerName = e.Customer?.Name ?? string.Empty,
        FiscalYearId = e.FiscalYearId,
        InvoiceDate = e.InvoiceDate,
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
        Lines = e.Lines.Select(l => new InvoiceLineResponseDto
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
            RevenueAccountId = l.RevenueAccountId,
            CostCenterId = l.CostCenterId
        }).ToList()
    };
}
