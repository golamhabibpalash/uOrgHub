using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.MaterialRequests.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.MaterialRequests.Queries;

public record GetMaterialRequestsQuery(PaginationRequest Request, Guid? ProjectId = null, MaterialRequestStatus? Status = null) : IQuery<PagedResult<MaterialRequestResponseDto>>;
public record GetMaterialRequestByIdQuery(Guid Id) : IQuery<MaterialRequestResponseDto>;
public record GetAllMaterialRequestsForExportQuery : IQuery<List<MaterialRequestResponseDto>>;

public class GetMaterialRequestsQueryHandler : IRequestHandler<GetMaterialRequestsQuery, PagedResult<MaterialRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetMaterialRequestsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<MaterialRequestResponseDto>> Handle(GetMaterialRequestsQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectMaterialRequest>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.RequestNumber);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.RequestDate)
            : query.OrderBy(x => x.RequestDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<MaterialRequestResponseDto>
        {
            Items = items.Select(MaterialRequestMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetMaterialRequestByIdQueryHandler : IRequestHandler<GetMaterialRequestByIdQuery, MaterialRequestResponseDto>
{
    private readonly AppDbContext _context;
    public GetMaterialRequestByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<MaterialRequestResponseDto> Handle(GetMaterialRequestByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMaterialRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMaterialRequest), request.Id);

        return MaterialRequestMapper.ToDto(entity);
    }
}

public class GetAllMaterialRequestsForExportQueryHandler : IRequestHandler<GetAllMaterialRequestsForExportQuery, List<MaterialRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllMaterialRequestsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<MaterialRequestResponseDto>> Handle(GetAllMaterialRequestsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectMaterialRequest>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.RequestDate)
            .Select(x => MaterialRequestMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
