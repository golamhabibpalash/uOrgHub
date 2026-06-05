using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Shared.Data;

namespace uOrgHub.Accounts.Features.FiscalYear;

public record GetAllFiscalYearsForExportQuery : IQuery<List<FiscalYearResponseDto>>;

public class GetAllFiscalYearsForExportQueryHandler : IRequestHandler<GetAllFiscalYearsForExportQuery, List<FiscalYearResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FiscalYearMapper _mapper = new();

    public GetAllFiscalYearsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<FiscalYearResponseDto>> Handle(GetAllFiscalYearsForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.FiscalYear>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(ct);
        return items.Select(_mapper.ToDto).ToList();
    }
}
