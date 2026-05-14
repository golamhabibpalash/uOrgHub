using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.Clients.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.Clients.Queries;

public record GetClientsQuery(PaginationRequest Request, ClientStatus? Status = null) : IQuery<PagedResult<ClientResponseDto>>;
public record GetClientByIdQuery(Guid Id) : IQuery<ClientResponseDto>;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, PagedResult<ClientResponseDto>>
{
    private readonly AppDbContext _context;
    public GetClientsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ClientResponseDto>> Handle(GetClientsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Client>().Where(x => !x.IsDeleted).AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.CompanyName.Contains(request.Request.Search)
                || x.ClientCode.Contains(request.Request.Search)
                || (x.ContactPerson != null && x.ContactPerson.Contains(request.Request.Search)));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.CompanyName)
            : query.OrderBy(x => x.CompanyName);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ClientResponseDto>
        {
            Items = items.Select(ClientMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientResponseDto>
{
    private readonly AppDbContext _context;
    public GetClientByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ClientResponseDto> Handle(GetClientByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<Client>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Client), request.Id);

        return ClientMapper.ToDto(entity);
    }
}
