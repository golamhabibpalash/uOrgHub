using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Shared.Data;

namespace uOrgHub.Accounts.Features.AccountGroup;

public record GetAllAccountGroupsForExportQuery : IQuery<List<AccountGroupResponseDto>>;

public class GetAllAccountGroupsForExportQueryHandler : IRequestHandler<GetAllAccountGroupsForExportQuery, List<AccountGroupResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly AccountGroupMapper _mapper = new();

    public GetAllAccountGroupsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<AccountGroupResponseDto>> Handle(GetAllAccountGroupsForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.AccountGroup>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        return items.Select(_mapper.ToDto).ToList();
    }
}
