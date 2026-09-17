using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.Documents.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Reporting.Documents;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.Documents.Commands;

public record UpdatePRDocumentCommand(Guid Id, UpdatePRDocumentDto Dto, Guid UserId) : ICommand<PRDocumentResponseDto>;
public record RegeneratePRDocumentCommand(Guid Id, Guid UserId) : ICommand<PRDocumentResponseDto>;
public record UpdateRfqDocumentCommand(Guid Id, UpdateRfqDocumentDto Dto, Guid UserId) : ICommand<RfqDocumentResponseDto>;
public record RegenerateRfqDocumentCommand(Guid Id, Guid UserId) : ICommand<RfqDocumentResponseDto>;

public class UpdatePRDocumentCommandHandler : IRequestHandler<UpdatePRDocumentCommand, PRDocumentResponseDto>
{
    private readonly AppDbContext _context;
    public UpdatePRDocumentCommandHandler(AppDbContext context) => _context = context;

    public async Task<PRDocumentResponseDto> Handle(UpdatePRDocumentCommand request, CancellationToken ct)
    {
        var pr = await _context.Set<PurchaseRequisition>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        if (string.IsNullOrWhiteSpace(request.Dto.DocumentText))
            throw new AppException("The application text cannot be empty.");

        pr.DocumentText = request.Dto.DocumentText!.Trim();
        pr.DocumentEditedAt = DateTime.UtcNow;
        pr.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return await new GetPRDocumentQueryHandler(_context).Handle(new GetPRDocumentQuery(request.Id, request.UserId), ct);
    }
}

public class RegeneratePRDocumentCommandHandler : IRequestHandler<RegeneratePRDocumentCommand, PRDocumentResponseDto>
{
    private readonly AppDbContext _context;
    public RegeneratePRDocumentCommandHandler(AppDbContext context) => _context = context;

    public async Task<PRDocumentResponseDto> Handle(RegeneratePRDocumentCommand request, CancellationToken ct)
    {
        var pr = await _context.Set<PurchaseRequisition>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        var dto = await new GetPRDocumentQueryHandler(_context).Handle(new GetPRDocumentQuery(request.Id, request.UserId), ct);

        pr.DocumentText = DocumentTextGenerator.GeneratePRBody(new PRDocumentSource(
            pr.PRNumber,
            dto.RequestedByName,
            dto.DepartmentName,
            pr.Purpose,
            pr.RequiredDate != default ? GetPRDocumentQueryHandler.FriendlyDate(pr.RequiredDate) : null,
            dto.Items.Select(i => new DocumentItemSource(i.VariantName, i.UOM, i.Quantity, i.Notes)).ToList()));
        pr.DocumentEditedAt = null;
        pr.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        dto.DocumentText = pr.DocumentText;
        dto.IsDocumentEdited = false;
        dto.DocumentEditedAt = null;
        return dto;
    }
}

public class UpdateRfqDocumentCommandHandler : IRequestHandler<UpdateRfqDocumentCommand, RfqDocumentResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateRfqDocumentCommandHandler(AppDbContext context) => _context = context;

    public async Task<RfqDocumentResponseDto> Handle(UpdateRfqDocumentCommand request, CancellationToken ct)
    {
        var rfq = await _context.Set<RequestForQuotation>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RequestForQuotation), request.Id);

        if (string.IsNullOrWhiteSpace(request.Dto.DocumentText))
            throw new AppException("The application text cannot be empty.");

        rfq.DocumentText = request.Dto.DocumentText!.Trim();
        rfq.DocumentEditedAt = DateTime.UtcNow;
        rfq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return await new GetRfqDocumentQueryHandler(_context).Handle(new GetRfqDocumentQuery(request.Id, request.UserId), ct);
    }
}

public class RegenerateRfqDocumentCommandHandler : IRequestHandler<RegenerateRfqDocumentCommand, RfqDocumentResponseDto>
{
    private readonly AppDbContext _context;
    public RegenerateRfqDocumentCommandHandler(AppDbContext context) => _context = context;

    public async Task<RfqDocumentResponseDto> Handle(RegenerateRfqDocumentCommand request, CancellationToken ct)
    {
        var rfq = await _context.Set<RequestForQuotation>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(RequestForQuotation), request.Id);

        var dto = await new GetRfqDocumentQueryHandler(_context).Handle(new GetRfqDocumentQuery(request.Id, request.UserId), ct);

        string? prNumber = null;
        if (rfq.PRId.HasValue)
            prNumber = await _context.Set<PurchaseRequisition>()
                .Where(x => x.Id == rfq.PRId.Value && !x.IsDeleted)
                .Select(x => x.PRNumber)
                .FirstOrDefaultAsync(ct);

        rfq.DocumentText = DocumentTextGenerator.GenerateRFQBody(new RfqDocumentSource(
            prNumber,
            rfq.Title,
            rfq.Description,
            rfq.ClosingDate != default ? GetPRDocumentQueryHandler.FriendlyDate(rfq.ClosingDate) : null,
            dto.Items.Select(i => new DocumentItemSource(i.VariantName, i.UOM, i.Quantity, i.Notes)).ToList()));
        rfq.DocumentEditedAt = null;
        rfq.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        dto.DocumentText = rfq.DocumentText;
        dto.IsDocumentEdited = false;
        dto.DocumentEditedAt = null;
        return dto;
    }
}