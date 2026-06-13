using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;

namespace uOrgHub.Accounts.Services;

public class DocumentNumberingService : IDocumentNumberingService
{
    private readonly AppDbContext _context;

    public DocumentNumberingService(AppDbContext context) => _context = context;

    public async Task<string> GenerateNextAsync(string documentType, string prefix, int? year = null, int? month = null)
    {
        var currentYear = year ?? DateTime.UtcNow.Year;
        var currentMonth = month ?? DateTime.UtcNow.Month;

        var sequence = await _context.Set<NumberingSequence>()
            .FirstOrDefaultAsync(x =>
                x.DocumentType == documentType &&
                x.Prefix == prefix &&
                x.Year == currentYear &&
                x.Month == currentMonth);

        if (sequence == null)
        {
            sequence = new NumberingSequence
            {
                DocumentType = documentType,
                Prefix = prefix,
                Year = currentYear,
                Month = currentMonth,
                LastSequence = 1,
                Pattern = $"{prefix}-{currentYear}{currentMonth:D2}-" + "{SEQ:6}"
            };
            _context.Set<NumberingSequence>().Add(sequence);
        }
        else
        {
            sequence.LastSequence++;
        }

        await _context.SaveChangesAsync();

        var seqStr = sequence.LastSequence.ToString("D6");
        return $"{prefix}-{currentYear}{currentMonth:D2}-{seqStr}";
    }
}
