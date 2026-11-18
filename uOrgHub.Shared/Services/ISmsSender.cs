namespace uOrgHub.Shared.Services;

/// <summary>
/// Queues an SMS through the current company's active SMS gateway, for any module that needs to
/// text someone (reminders, notifications). Lives in Shared so modules can depend on it without
/// referencing uOrgHub.Sms; implemented there. The message is sent by a background worker and
/// recorded in SMS history under <paramref name="source"/>.
/// </summary>
public interface ISmsSender
{
    /// <param name="idempotencyKey">Same key twice queues once — pass something stable for the event (e.g. "invoice-reminder:{id}:{date}").</param>
    /// <returns>The send request's ID.</returns>
    Task<Guid> QueueAsync(IReadOnlyList<string> recipients, string message, string source, string idempotencyKey, CancellationToken ct = default);
}
