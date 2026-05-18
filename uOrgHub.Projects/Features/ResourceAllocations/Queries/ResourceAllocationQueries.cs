using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.ResourceAllocations.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.ResourceAllocations.Queries;

public record GetResourceAllocationsQuery(PaginationRequest Request, Guid? ProjectId = null, ResourceType? ResourceType = null, ResourceAllocationStatus? Status = null) : IQuery<PagedResult<ResourceAllocationResponseDto>>;
public record GetResourceAllocationByIdQuery(Guid Id) : IQuery<ResourceAllocationResponseDto>;

public class GetResourceAllocationsQueryHandler : IRequestHandler<GetResourceAllocationsQuery, PagedResult<ResourceAllocationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetResourceAllocationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ResourceAllocationResponseDto>> Handle(GetResourceAllocationsQuery request, CancellationToken ct)
    {
        var query = _context.Set<SiteResourceAllocation>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.ResourceType.HasValue)
            query = query.Where(x => x.ResourceType == request.ResourceType.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Description.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.PlannedStartDate)
            : query.OrderBy(x => x.PlannedStartDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ResourceAllocationResponseDto>
        {
            Items = items.Select(ResourceAllocationMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetResourceAllocationByIdQueryHandler : IRequestHandler<GetResourceAllocationByIdQuery, ResourceAllocationResponseDto>
{
    private readonly AppDbContext _context;
    public GetResourceAllocationByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ResourceAllocationResponseDto> Handle(GetResourceAllocationByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<SiteResourceAllocation>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SiteResourceAllocation), request.Id);

        return ResourceAllocationMapper.ToDto(entity);
    }
}
