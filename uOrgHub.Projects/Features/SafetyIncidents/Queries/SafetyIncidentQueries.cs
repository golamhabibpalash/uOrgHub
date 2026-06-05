using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.SafetyIncidents.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.SafetyIncidents.Queries;

public record GetSafetyIncidentsQuery(PaginationRequest Request, Guid? ProjectId = null, SafetyIncidentSeverity? Severity = null, SafetyIncidentStatus? Status = null) : IQuery<PagedResult<SafetyIncidentResponseDto>>;
public record GetSafetyIncidentByIdQuery(Guid Id) : IQuery<SafetyIncidentResponseDto>;
public record GetAllSafetyIncidentsForExportQuery : IQuery<List<SafetyIncidentResponseDto>>;

public class GetSafetyIncidentsQueryHandler : IRequestHandler<GetSafetyIncidentsQuery, PagedResult<SafetyIncidentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetSafetyIncidentsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<SafetyIncidentResponseDto>> Handle(GetSafetyIncidentsQuery request, CancellationToken ct)
    {
        var query = _context.Set<SafetyIncident>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Severity.HasValue)
            query = query.Where(x => x.Severity == request.Severity.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.IncidentNumber);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.IncidentDate)
            : query.OrderBy(x => x.IncidentDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<SafetyIncidentResponseDto>
        {
            Items = items.Select(SafetyIncidentMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetSafetyIncidentByIdQueryHandler : IRequestHandler<GetSafetyIncidentByIdQuery, SafetyIncidentResponseDto>
{
    private readonly AppDbContext _context;
    public GetSafetyIncidentByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<SafetyIncidentResponseDto> Handle(GetSafetyIncidentByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<SafetyIncident>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SafetyIncident), request.Id);

        return SafetyIncidentMapper.ToDto(entity);
    }
}

public class GetAllSafetyIncidentsForExportQueryHandler : IRequestHandler<GetAllSafetyIncidentsForExportQuery, List<SafetyIncidentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllSafetyIncidentsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<SafetyIncidentResponseDto>> Handle(GetAllSafetyIncidentsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<SafetyIncident>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.IncidentDate)
            .Select(x => SafetyIncidentMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
