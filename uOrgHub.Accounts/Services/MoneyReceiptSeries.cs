using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Accounts.Services;

/// <summary>
/// The MR No. series for money receipts: one plain running number per company (sister concern),
/// never reset by month or year, so it can carry on from the pre-printed paper MR books. Stored as
/// a <see cref="NumberingSequence"/> row, which is company-scoped like every other sequence.
/// </summary>
public static class MoneyReceiptSeries
{
    private const string DocumentType = "MoneyReceipt";
    private const string Prefix = "MR";

    private static Task<NumberingSequence?> FindAsync(AppDbContext context, CancellationToken ct)
        => context.Set<NumberingSequence>()
            .FirstOrDefaultAsync(x => x.DocumentType == DocumentType && x.Prefix == Prefix, ct);

    /// <summary>The MR No. the next receipt will get.</summary>
    public static async Task<int> PeekNextAsync(AppDbContext context, CancellationToken ct)
        => ((await FindAsync(context, ct))?.LastSequence ?? 0) + 1;

    /// <summary>Set where the series continues — e.g. one after the last paper receipt issued.</summary>
    public static async Task SetNextAsync(AppDbContext context, int next, CancellationToken ct)
    {
        if (next < 1)
            throw new AppException("The next MR No. must be 1 or more.");

        var used = await HighestUsedAsync(context, ct);
        if (next <= used)
            throw new AppException($"MR No. {used} has already been issued. The next MR No. must be at least {used + 1}.");

        var seq = await FindAsync(context, ct) ?? Add(context);
        seq.LastSequence = next - 1;
        await context.SaveChangesAsync(ct);
    }

    /// <summary>Issues the next MR No.</summary>
    public static async Task<string> TakeNextAsync(AppDbContext context, CancellationToken ct)
    {
        var seq = await FindAsync(context, ct) ?? Add(context);
        int candidate;
        do
        {
            candidate = ++seq.LastSequence;
        } while (await IsTakenAsync(context, candidate.ToString(), ct)); // skip numbers typed in by hand
        await context.SaveChangesAsync(ct);
        return candidate.ToString();
    }

    /// <summary>
    /// Accepts an MR No. typed from a paper receipt already handed over. It must not be used before
    /// (voided receipts included — a cancelled MR No. is never reissued), and the series moves past it.
    /// </summary>
    public static async Task<string> UseManualAsync(AppDbContext context, string mrNumber, CancellationToken ct)
    {
        var value = mrNumber.Trim();
        if (value.Length > 30)
            throw new AppException("MR No. must be 30 characters or fewer.");
        if (await IsTakenAsync(context, value, ct))
            throw new AppException($"MR No. {value} has already been issued.");

        if (int.TryParse(value, out var n))
        {
            var seq = await FindAsync(context, ct) ?? Add(context);
            if (n > seq.LastSequence) seq.LastSequence = n;
        }
        return value;
    }

    // The global filter only scopes by company (soft-deleted rows are filtered per query), so this
    // already stays inside the current sister concern while still counting voided receipts.
    private static Task<bool> IsTakenAsync(AppDbContext context, string value, CancellationToken ct)
        => context.Set<Payment>().AnyAsync(p => p.MoneyReceiptNumber == value, ct);

    private static async Task<int> HighestUsedAsync(AppDbContext context, CancellationToken ct)
    {
        var numbers = await context.Set<Payment>()
            .Where(p => p.MoneyReceiptNumber != null)
            .Select(p => p.MoneyReceiptNumber!)
            .ToListAsync(ct);
        return numbers.Select(n => int.TryParse(n, out var v) ? v : 0).DefaultIfEmpty(0).Max();
    }

    private static NumberingSequence Add(AppDbContext context)
    {
        var seq = new NumberingSequence
        {
            DocumentType = DocumentType,
            Prefix = Prefix,
            Year = 0,
            Month = null,
            LastSequence = 0,
            Pattern = "{SEQ}",
        };
        context.Set<NumberingSequence>().Add(seq);
        return seq;
    }
}
