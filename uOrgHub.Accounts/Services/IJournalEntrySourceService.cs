namespace uOrgHub.Accounts.Services;

/// <summary>
/// The document a journal entry was generated from, when it was not written by hand.
/// </summary>
/// <param name="DocumentType">"Voucher", "Bill", "Invoice" or "Payment".</param>
/// <param name="DocumentNumber">The document's own number, e.g. "DR-202608-000001".</param>
/// <param name="DocumentStatus">Where that document currently sits in its workflow.</param>
public record JournalEntrySource(string DocumentType, string DocumentNumber, string DocumentStatus);

public interface IJournalEntrySourceService
{
    /// <summary>The document that owns this entry, or null when it was entered by hand.</summary>
    Task<JournalEntrySource?> FindSourceAsync(Guid journalEntryId, CancellationToken ct = default);

    /// <summary>
    /// Sources for a whole page of entries in one pass, so a list does not issue a lookup per row.
    /// Hand-written entries are simply absent from the result.
    /// </summary>
    Task<Dictionary<Guid, JournalEntrySource>> FindSourcesAsync(
        IReadOnlyCollection<Guid> journalEntryIds, CancellationToken ct = default);

    /// <summary>
    /// Throws when the entry belongs to a document, because the document's own workflow is the
    /// only thing allowed to post, edit, delete or cancel it. Call this from paths a user reaches
    /// directly — the Journal Entries screen — never from the owning workflow itself.
    /// </summary>
    Task EnsureNotDocumentOwnedAsync(Guid journalEntryId, string action, CancellationToken ct = default);
}
