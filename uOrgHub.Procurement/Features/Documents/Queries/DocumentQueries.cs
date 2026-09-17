using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Reporting.Documents;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Procurement.Features.Documents.Queries;

public record GetPRDocumentQuery(Guid Id, Guid UserId) : IQuery<PRDocumentResponseDto>;
public record GetRfqDocumentQuery(Guid Id, Guid UserId) : IQuery<RfqDocumentResponseDto>;

public class GetPRDocumentQueryHandler : IRequestHandler<GetPRDocumentQuery, PRDocumentResponseDto>
{
    private readonly AppDbContext _context;
    public GetPRDocumentQueryHandler(AppDbContext context) => _context = context;

    public async Task<PRDocumentResponseDto> Handle(GetPRDocumentQuery request, CancellationToken ct)
    {
        var company = await CurrentCompanyAsync(request.UserId, ct);
        var pr = await _context.Set<PurchaseRequisition>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

        var variants = await LoadVariantsAsync(pr.Items.Select(i => i.ItemVariantId).Distinct().ToList(), ct);
        var empIds = new List<Guid> { pr.RequestedById };
        if (pr.ApprovedById.HasValue) empIds.Add(pr.ApprovedById.Value);
        var emps = await _context.Set<Employee>().Where(x => empIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var depts = await _context.Set<Department>().Where(x => x.Id == pr.DepartmentId)
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var dtos = pr.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).Select((item, idx) => new DocumentItemDto
        {
            LineNo = idx + 1,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v) ? v.VariantName : string.Empty,
            Description = variants.TryGetValue(item.ItemVariantId, out var v2) ? v2.Item.Description : null,
            UOM = variants.TryGetValue(item.ItemVariantId, out var v3) ? v3.Item.UnitOfMeasure?.Abbreviation : null,
            Quantity = item.RequestedQuantity,
            UnitCost = item.EstimatedUnitCost,
            TotalCost = item.EstimatedTotalCost,
            Notes = item.Notes
        }).ToList();

        var dto = new PRDocumentResponseDto
        {
            Id = pr.Id, PRNumber = pr.PRNumber, PRDate = pr.PRDate, RequiredDate = pr.RequiredDate,
            DepartmentName = depts.GetValueOrDefault(pr.DepartmentId, string.Empty),
            RequestedByName = emps.GetValueOrDefault(pr.RequestedById, string.Empty),
            Purpose = pr.Purpose, Status = pr.Status,
            ApprovedByName = pr.ApprovedById.HasValue ? emps.GetValueOrDefault(pr.ApprovedById.Value) : null,
            ApprovedAt = pr.ApprovedAt, Notes = pr.Notes,
            DocumentText = pr.DocumentText,
            IsDocumentEdited = pr.DocumentEditedAt.HasValue,
            DocumentEditedAt = pr.DocumentEditedAt,
            TotalEstimatedCost = dtos.Where(i => i.TotalCost.HasValue).Sum(i => i.TotalCost!.Value),
            Company = ToCompanyInfo(company),
            Items = dtos
        };

        if (string.IsNullOrWhiteSpace(dto.DocumentText))
        {
            dto.DocumentText = DocumentTextGenerator.GeneratePRBody(new PRDocumentSource(
                pr.PRNumber,
                dto.RequestedByName,
                dto.DepartmentName,
                pr.Purpose,
                pr.RequiredDate != default ? FriendlyDate(pr.RequiredDate) : null,
                dtos.Select(i => new DocumentItemSource(i.VariantName, i.UOM, i.Quantity, i.Notes)).ToList()));

            pr.DocumentText = dto.DocumentText;
            pr.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        return dto;
    }

    internal async Task<Company> CurrentCompanyAsync(Guid userId, CancellationToken ct)
    {
        var company = await _context.Set<UserCompany>()
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId && !uc.IsDeleted)
            .Select(uc => uc.Company)
            .FirstOrDefaultAsync(ct);
        return company ?? await _context.Set<Company>().Where(c => !c.IsDeleted).OrderBy(c => c.CreatedAt).FirstOrDefaultAsync(ct)
            ?? throw new AppException("No company found for the current user.");
    }

    internal async Task<Dictionary<Guid, ItemVariant>> LoadVariantsAsync(List<Guid> variantIds, CancellationToken ct)
    {
        return await _context.Set<ItemVariant>()
            .Include(x => x.Item).ThenInclude(i => i.UnitOfMeasure)
            .Where(x => variantIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }

    internal static string FriendlyDate(DateTime date) => date.ToString("MMM d, yyyy");

    internal static CompanyInfoDto ToCompanyInfo(Company c) => new()
    {
        Name = c.Name, TagLine = c.TagLine, Address = c.Address, Phone = c.Phone, Email = c.Email,
        LogoUrl = c.LogoUrl, Currency = c.Currency
    };
}

public class GetRfqDocumentQueryHandler : IRequestHandler<GetRfqDocumentQuery, RfqDocumentResponseDto>
{
    private readonly AppDbContext _context;
    public GetRfqDocumentQueryHandler(AppDbContext context) => _context = context;

    public async Task<RfqDocumentResponseDto> Handle(GetRfqDocumentQuery request, CancellationToken ct)
    {
        var company = await new GetPRDocumentQueryHandler(_context).CurrentCompanyAsync(request.UserId, ct);
        var rfq = await _context.Set<RequestForQuotation>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(RequestForQuotation), request.Id);

        var variants = await new GetPRDocumentQueryHandler(_context).LoadVariantsAsync(rfq.Items.Select(i => i.ItemVariantId).Distinct().ToList(), ct);

        string? prNumber = null;
        string? prPurpose = null;
        if (rfq.PRId.HasValue)
        {
            var pr = await _context.Set<PurchaseRequisition>()
                .Where(x => !x.IsDeleted && x.Id == rfq.PRId.Value)
                .Select(x => new { x.PRNumber, x.Purpose })
                .FirstOrDefaultAsync(ct);
            prNumber = pr?.PRNumber;
            prPurpose = pr?.Purpose;
        }

        var dtos = rfq.Items.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).Select((item, idx) => new DocumentItemDto
        {
            LineNo = idx + 1,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v) ? v.VariantName : string.Empty,
            Description = variants.TryGetValue(item.ItemVariantId, out var v2) ? v2.Item.Description : null,
            UOM = variants.TryGetValue(item.ItemVariantId, out var v3) ? v3.Item.UnitOfMeasure?.Abbreviation : null,
            Quantity = item.RequestedQuantity,
            Notes = item.Notes
        }).ToList();

        var dto = new RfqDocumentResponseDto
        {
            Id = rfq.Id, RFQNumber = rfq.RFQNumber, RFQDate = rfq.RFQDate, ClosingDate = rfq.ClosingDate,
            Title = rfq.Title, Description = rfq.Description,
            PRNumber = prNumber, PRPurpose = prPurpose,
            Status = rfq.Status, Notes = rfq.Notes,
            DocumentText = rfq.DocumentText,
            IsDocumentEdited = rfq.DocumentEditedAt.HasValue,
            DocumentEditedAt = rfq.DocumentEditedAt,
            Company = GetPRDocumentQueryHandler.ToCompanyInfo(company),
            Items = dtos
        };

        if (string.IsNullOrWhiteSpace(dto.DocumentText))
        {
            dto.DocumentText = DocumentTextGenerator.GenerateRFQBody(new RfqDocumentSource(
                prNumber,
                rfq.Title,
                rfq.Description,
                rfq.ClosingDate != default ? GetPRDocumentQueryHandler.FriendlyDate(rfq.ClosingDate) : null,
                dtos.Select(i => new DocumentItemSource(i.VariantName, i.UOM, i.Quantity, i.Notes)).ToList()));

            rfq.DocumentText = dto.DocumentText;
            rfq.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        return dto;
    }
}