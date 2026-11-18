namespace uOrgHub.Shared.Services;

/// <summary>
/// Queues an SMS through the current company's active SMS gateway, for any module that needs to
/// text someone (receipts, reminders). Lives in Shared so modules can depend on it without
/// referencing uOrgHub.Sms; implemented there. The message is sent by a background worker and
/// recorded in SMS history under <paramref name="source"/>.
/// </summary>
public interface ISmsSender
{
    /// <param name="idempotencyKey">Same key twice queues once — pass a key per user action, not per document.</param>
    /// <param name="sourceReference">The document behind the SMS (e.g. MR or voucher number), shown in SMS history.</param>
    /// <returns>The send request's ID.</returns>
    /// <exception cref="Exceptions.AppException">No active gateway, gateway disabled, or invalid input.</exception>
    Task<Guid> QueueAsync(
        IReadOnlyList<string> recipients, string message, string source, string idempotencyKey,
        string? sourceReference = null, CancellationToken ct = default);

    /// <summary>The number in its stored form, or null when it is not a valid mobile number.</summary>
    string? NormalizeRecipient(string? phone);
}
