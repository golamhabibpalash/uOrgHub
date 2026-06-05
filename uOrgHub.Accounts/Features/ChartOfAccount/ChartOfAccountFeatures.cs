using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Shared.Data;

namespace uOrgHub.Accounts.Features.ChartOfAccount;

public record GetAllChartOfAccountsForExportQuery : IQuery<List<ChartOfAccountResponseDto>>;

public class GetAllChartOfAccountsForExportQueryHandler : IRequestHandler<GetAllChartOfAccountsForExportQuery, List<ChartOfAccountResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly ChartOfAccountMapper _mapper = new();

    public GetAllChartOfAccountsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ChartOfAccountResponseDto>> Handle(GetAllChartOfAccountsForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.ChartOfAccount>()
            .Include(x => x.AccountGroup)
            .Include(x => x.ParentAccount)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.AccountCode)
            .ToListAsync(ct);
        return items.Select(_mapper.ToDto).ToList();
    }
}
