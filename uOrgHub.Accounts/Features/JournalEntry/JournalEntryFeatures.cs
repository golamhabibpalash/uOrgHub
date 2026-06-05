using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Shared.Data;

namespace uOrgHub.Accounts.Features.JournalEntry;

public record GetAllJournalEntriesForExportQuery : IQuery<List<JournalEntryResponseDto>>;

public class GetAllJournalEntriesForExportQueryHandler : IRequestHandler<GetAllJournalEntriesForExportQuery, List<JournalEntryResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly JournalEntryMapper _mapper = new();

    public GetAllJournalEntriesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<JournalEntryResponseDto>> Handle(GetAllJournalEntriesForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.JournalEntry>()
            .Include(x => x.Lines)
                .ThenInclude(l => l.Account)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.EntryDate)
            .ToListAsync(ct);
        return items.Select(_mapper.ToDto).ToList();
    }
}
