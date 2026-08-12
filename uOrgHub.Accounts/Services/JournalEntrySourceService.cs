using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Accounts.Services;

/// <summary>
/// Answers "where did this journal entry come from?" and enforces the answer.
///
/// Four documents generate entries — Voucher, Bill, Invoice and Payment — and each has its own
/// approval workflow that decides when the entry may be posted, reversed or discarded. An entry
/// reached directly from the Journal Entries screen bypasses that workflow entirely: a voucher
/// still awaiting approval would have its money hit the ledger, and the voucher would go on
/// showing "Submitted" for a transaction that had already posted. Ownership is what closes that.
/// </summary>
public class JournalEntrySourceService : IJournalEntrySourceService
{
    private readonly AppDbContext _db;

    public JournalEntrySourceService(AppDbContext db) => _db = db;

    public async Task<JournalEntrySource?> FindSourceAsync(Guid journalEntryId, CancellationToken ct = default)
    {
        var sources = await FindSourcesAsync(new[] { journalEntryId }, ct);
        return sources.TryGetValue(journalEntryId, out var source) ? source : null;
    }

    public async Task<Dictionary<Guid, JournalEntrySource>> FindSourcesAsync(
        IReadOnlyCollection<Guid> journalEntryIds, CancellationToken ct = default)
    {
        var result = new Dictionary<Guid, JournalEntrySource>();
        if (journalEntryIds.Count == 0)
            return result;

        var ids = journalEntryIds.Distinct().ToList();

        // One query per document type rather than one per entry: a page of 100 entries costs four
        // round trips, not four hundred. Each projects to the link id, so the filtering happens in
        // the database instead of pulling every document back to sift here.
        var vouchers = await _db.Set<Voucher>()
            .Where(x => !x.IsDeleted && x.JournalEntryId != null && ids.Contains(x.JournalEntryId.Value))
            .Select(x => new { EntryId = x.JournalEntryId!.Value, Number = x.VoucherNumber, Status = x.Status.ToString() })
            .ToListAsync(ct);

        var bills = await _db.Set<Bill>()
            .Where(x => !x.IsDeleted && x.JournalEntryId != null && ids.Contains(x.JournalEntryId.Value))
            .Select(x => new { EntryId = x.JournalEntryId!.Value, Number = x.BillNumber, Status = x.Status.ToString() })
            .ToListAsync(ct);

        var invoices = await _db.Set<Invoice>()
            .Where(x => !x.IsDeleted && x.JournalEntryId != null && ids.Contains(x.JournalEntryId.Value))
            .Select(x => new { EntryId = x.JournalEntryId!.Value, Number = x.InvoiceNumber, Status = x.Status.ToString() })
            .ToListAsync(ct);

        // A payment carries no status column: voiding one soft-deletes it, so every payment still
        // visible here is simply live. "Recorded" says that without inventing a workflow it lacks.
        var payments = await _db.Set<Payment>()
            .Where(x => !x.IsDeleted && x.JournalEntryId != null && ids.Contains(x.JournalEntryId.Value))
            .Select(x => new { EntryId = x.JournalEntryId!.Value, Number = x.PaymentNumber, Status = "Recorded" })
            .ToListAsync(ct);

        // First writer wins. An entry can only legitimately belong to one document, so a second
        // claim would be data corruption rather than something to merge.
        foreach (var x in vouchers) result.TryAdd(x.EntryId, new JournalEntrySource("Voucher", x.Number, x.Status));
        foreach (var x in bills) result.TryAdd(x.EntryId, new JournalEntrySource("Bill", x.Number, x.Status));
        foreach (var x in invoices) result.TryAdd(x.EntryId, new JournalEntrySource("Invoice", x.Number, x.Status));
        foreach (var x in payments) result.TryAdd(x.EntryId, new JournalEntrySource("Payment", x.Number, x.Status));

        return result;
    }

    public async Task EnsureNotDocumentOwnedAsync(Guid journalEntryId, string action, CancellationToken ct = default)
    {
        var source = await FindSourceAsync(journalEntryId, ct);
        if (source is null)
            return;

        throw new AppException(BuildMessage(source, action));
    }

    /// <summary>
    /// Says what owns the entry and where to go instead. The document's current status is included
    /// because it is the thing that decides what the user can do next — being told a voucher owns
    /// the entry is only half an answer if they cannot see that it is still awaiting approval.
    /// </summary>
    private static string BuildMessage(JournalEntrySource source, string action)
    {
        var where = source.DocumentType switch
        {
            "Voucher" => "Approve and post the voucher instead — posting it posts this entry.",
            "Bill" => "Use the bill's own approve or void action instead.",
            "Invoice" => "Use the invoice's own post or void action instead.",
            "Payment" => "Use the payment's own void action instead.",
            _ => "Use the source document's own workflow instead.",
        };

        return $"This journal entry was generated by {source.DocumentType} " +
               $"{source.DocumentNumber} (currently {source.DocumentStatus}) and cannot be {action} directly. {where}";
    }

}
