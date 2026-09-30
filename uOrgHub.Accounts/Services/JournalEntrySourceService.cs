using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Accounts.Services;

/// <summary>
/// Answers "where did this journal entry come from?" and enforces the answer.
///
/// Six documents generate entries — Voucher, Bill, Invoice, Payment, Depreciation run and Equipment Hire run — and each has its own
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

        // A voucher's reversal ("Correct this voucher") is part of the voucher's history too — it
        // must not be cancelled on its own, or the wrong original amount silently comes back.
        var voucherReversals = await _db.Set<Voucher>()
            .Where(x => !x.IsDeleted && x.ReversalJournalEntryId != null && ids.Contains(x.ReversalJournalEntryId.Value))
            .Select(x => new { EntryId = x.ReversalJournalEntryId!.Value, Number = x.VoucherNumber, Status = x.Status.ToString() })
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

        // A depreciation run's entry must be unwound through the run, which also rolls back each
        // asset's accumulated depreciation — cancelling the entry alone would leave the register
        // claiming depreciation the ledger no longer carries.
        var depreciationRuns = await _db.Set<DepreciationRun>()
            .Where(x => !x.IsDeleted && x.JournalEntryId != null && ids.Contains(x.JournalEntryId.Value))
            .Select(x => new { EntryId = x.JournalEntryId!.Value, Number = x.RunNumber, Status = x.Status.ToString() })
            .ToListAsync(ct);

        // Same reasoning for internal equipment hire: reversing the run is what frees its date
        // range to be charged again.
        var hireRuns = await _db.Set<HireChargeRun>()
            .Where(x => !x.IsDeleted && x.JournalEntryId != null && ids.Contains(x.JournalEntryId.Value))
            .Select(x => new { EntryId = x.JournalEntryId!.Value, Number = x.RunNumber, Status = x.Status.ToString() })
            .ToListAsync(ct);

        // First writer wins. An entry can only legitimately belong to one document, so a second
        // claim would be data corruption rather than something to merge.
        foreach (var x in vouchers) result.TryAdd(x.EntryId, new JournalEntrySource("Voucher", x.Number, x.Status));
        foreach (var x in voucherReversals) result.TryAdd(x.EntryId, new JournalEntrySource("Voucher", x.Number, x.Status));
        foreach (var x in bills) result.TryAdd(x.EntryId, new JournalEntrySource("Bill", x.Number, x.Status));
        foreach (var x in invoices) result.TryAdd(x.EntryId, new JournalEntrySource("Invoice", x.Number, x.Status));
        foreach (var x in payments) result.TryAdd(x.EntryId, new JournalEntrySource("Payment", x.Number, x.Status));
        foreach (var x in depreciationRuns) result.TryAdd(x.EntryId, new JournalEntrySource("Depreciation", x.Number, x.Status));
        foreach (var x in hireRuns) result.TryAdd(x.EntryId, new JournalEntrySource("Equipment Hire", x.Number, x.Status));

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
            "Voucher" when source.DocumentStatus == nameof(Models.Enums.VoucherStatus.Reversed)
                => "It records the voucher's correction and stays as part of its history.",
            "Voucher" => "Approve and post the voucher instead — posting it posts this entry.",
            "Bill" => "Use the bill's own approve or void action instead.",
            "Invoice" => "Use the invoice's own post or void action instead.",
            "Payment" => "Use the payment's own void action instead.",
            "Depreciation" => "Reverse the depreciation run instead — that also restores each asset's accumulated depreciation.",
            "Equipment Hire" => "Reverse the hire charge run instead — that also frees its dates to be charged again.",
            _ => "Use the source document's own workflow instead.",
        };

        return $"This journal entry was generated by {source.DocumentType} " +
               $"{source.DocumentNumber} (currently {source.DocumentStatus}) and cannot be {action} directly. {where}";
    }

}
