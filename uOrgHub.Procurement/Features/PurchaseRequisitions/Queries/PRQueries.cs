using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Procurement.Features.PurchaseRequisitions.Queries;

public record GetPRsQuery(PaginationRequest Request, PRStatus? Status = null) : IQuery<PagedResult<PRResponseDto>>;
public record GetPRByIdQuery(Guid Id) : IQuery<PRResponseDto>;

public class GetPRsQueryHandler : IRequestHandler<GetPRsQuery, PagedResult<PRResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPRsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<PRResponseDto>> Handle(GetPRsQuery request, CancellationToken ct)
    {
        var query = _context.Set<PurchaseRequisition>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.PRNumber.Contains(request.Request.Search) ||
                                     (x.Purpose != null && x.Purpose.Contains(request.Request.Search)));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.PRDate) : query.OrderBy(x => x.PRDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        var deptIds = items.Select(x => x.DepartmentId).Distinct().ToList();
        var empIds = items.SelectMany(x => new[] { x.RequestedById }.Concat(x.ApprovedById.HasValue ? new[] { x.ApprovedById.Value } : Array.Empty<Guid>())).Distinct().ToList();
        var variantIds = items.SelectMany(x => x.Items.Select(i => i.ItemVariantId)).Distinct().ToList();
        var warehouseIds = items.SelectMany(x => x.Items.Select(i => i.WarehouseId)).Distinct().ToList();

        var depts = await _context.Set<Department>().Where(x => deptIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var emps = await _context.Set<Employee>().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);
        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return new PagedResult<PRResponseDto>
        {
            Items = items.Select(pr => BuildDto(pr, depts, emps, variants, warehouses)).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    internal static PRResponseDto BuildDto(PurchaseRequisition pr,
        Dictionary<Guid, string> depts,
        Dictionary<Guid, string> emps,
        Dictionary<Guid, VariantInfo> variants,
        Dictionary<Guid, string> warehouses) => new()
    {
        Id = pr.Id, PRNumber = pr.PRNumber, PRDate = pr.PRDate, RequiredDate = pr.RequiredDate,
        DepartmentId = pr.DepartmentId, DepartmentName = depts.GetValueOrDefault(pr.DepartmentId, string.Empty),
        RequestedById = pr.RequestedById, RequestedByName = emps.GetValueOrDefault(pr.RequestedById, string.Empty),
        Purpose = pr.Purpose, Status = pr.Status,
        ApprovedById = pr.ApprovedById, ApprovedByName = pr.ApprovedById.HasValue ? emps.GetValueOrDefault(pr.ApprovedById.Value) : null,
        ApprovedAt = pr.ApprovedAt, RejectionReason = pr.RejectionReason, Notes = pr.Notes, CreatedAt = pr.CreatedAt,
        Items = pr.Items.Where(i => !i.IsDeleted).Select(item => new PRItemResponseDto
        {
            Id = item.Id, ItemVariantId = item.ItemVariantId,
            VariantSKU = variants.TryGetValue(item.ItemVariantId, out var v) ? v.SKU : string.Empty,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v2) ? v2.VariantName : string.Empty,
            WarehouseId = item.WarehouseId, WarehouseName = warehouses.GetValueOrDefault(item.WarehouseId, string.Empty),
            RequestedQuantity = item.RequestedQuantity, EstimatedUnitCost = item.EstimatedUnitCost,
            EstimatedTotalCost = item.EstimatedTotalCost, Notes = item.Notes
        }).ToList()
    };
}

public class GetPRByIdQueryHandler : IRequestHandler<GetPRByIdQuery, PRResponseDto>
{
    private readonly AppDbContext _context;
    public GetPRByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<PRResponseDto> Handle(GetPRByIdQuery request, CancellationToken ct)
    {
        var pr = await _context.Set<PurchaseRequisition>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(PurchaseRequisition), request.Id);

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
