using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Repositories;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly AppDbContext _context;

    public JournalEntryRepository(AppDbContext context) => _context = context;

    private IQueryable<JournalEntry> BaseQuery()
        => _context.Set<JournalEntry>()
            .Include(x => x.Lines)
            .ThenInclude(l => l.Account)
            .Where(x => !x.IsDeleted);

    public async Task<JournalEntry?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<JournalEntry>> GetAllAsync(PaginationRequest request)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.WhereSearch(request.Search, x => x.EntryNumber, x => x.Description, x => x.ReferenceNumber);

        query = request.SortDescending
            ? query.OrderByDescending(x => x.EntryDate)
            : query.OrderBy(x => x.EntryDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<JournalEntry> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<JournalEntry> CreateAsync(JournalEntry entity)
    {
        _context.Set<JournalEntry>().Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Collection(x => x.Lines).LoadAsync();
        foreach (var line in entity.Lines)
            await _context.Entry(line).Reference(l => l.Account).LoadAsync();
        return entity;
    }

    public async Task<JournalEntry> UpdateAsync(JournalEntry entity)
    {
        _context.Set<JournalEntry>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<JournalEntry>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<bool> EntryNumberExistsAsync(string entryNumber, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.EntryNumber == entryNumber && (excludeId == null || x.Id != excludeId));

    public async Task<string> GenerateEntryNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"JV-{year}-";
        
        var lastEntry = await _context.Set<JournalEntry>()
            .Where(x => x.EntryNumber.StartsWith(prefix))
            .OrderByDescending(x => x.EntryNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastEntry != null)
        {
            var lastSequence = lastEntry.EntryNumber.Split('-').Last();
            if (int.TryParse(lastSequence, out int lastNum))
                sequence = lastNum + 1;
        }

        return $"{prefix}{sequence:D4}";
    }
}