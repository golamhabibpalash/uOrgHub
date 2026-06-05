using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.Submittals.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.Submittals.Queries;

public record GetSubmittalsQuery(PaginationRequest Request, Guid? ProjectId = null, SubmittalStatus? Status = null) : IQuery<PagedResult<SubmittalResponseDto>>;
public record GetSubmittalByIdQuery(Guid Id) : IQuery<SubmittalResponseDto>;
public record GetAllSubmittalsForExportQuery : IQuery<List<SubmittalResponseDto>>;

public class GetSubmittalsQueryHandler : IRequestHandler<GetSubmittalsQuery, PagedResult<SubmittalResponseDto>>
{
    private readonly AppDbContext _context;
    public GetSubmittalsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<SubmittalResponseDto>> Handle(GetSubmittalsQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectSubmittal>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Title, x => x.SubmittalNumber);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.SubmittalNumber);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<SubmittalResponseDto>
        {
            Items = items.Select(SubmittalMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetSubmittalByIdQueryHandler : IRequestHandler<GetSubmittalByIdQuery, SubmittalResponseDto>
{
    private readonly AppDbContext _context;
    public GetSubmittalByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<SubmittalResponseDto> Handle(GetSubmittalByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectSubmittal>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectSubmittal), request.Id);

        return SubmittalMapper.ToDto(entity);
    }
}

public class GetAllSubmittalsForExportQueryHandler : IRequestHandler<GetAllSubmittalsForExportQuery, List<SubmittalResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllSubmittalsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<SubmittalResponseDto>> Handle(GetAllSubmittalsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectSubmittal>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SubmittalNumber)
            .Select(x => SubmittalMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
