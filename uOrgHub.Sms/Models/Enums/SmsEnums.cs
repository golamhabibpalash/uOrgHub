namespace uOrgHub.Sms.Models.Enums;

/// <summary>Character set a message is sent in. Decides the per-segment character limit.</summary>
public enum SmsEncoding
{
    /// <summary>GSM 03.38 default alphabet: 160 characters in one SMS, 153 per part when split.</summary>
    Gsm7,
    /// <summary>UCS-2 (needed for Bangla and emoji): 70 characters in one SMS, 67 per part when split.</summary>
    Unicode,
}

/// <summary>
/// Where one recipient's message is in its life. Queued and Sending are internal; the rest are
/// what the gateway told us.
/// </summary>
public enum SmsMessageStatus
{
    /// <summary>Saved, waiting for the dispatcher (also: waiting to retry after a transient failure).</summary>
    Queued,
    /// <summary>Handed to the gateway, response not yet back.</summary>
    Sending,
    /// <summary>The gateway accepted it. Final for providers without delivery reports.</summary>
    Submitted,
    /// <summary>The provider's delivery report says it reached the handset / operator.</summary>
    Delivered,
    /// <summary>Rejected by the gateway, or reported undelivered.</summary>
    Failed,
    /// <summary>
    /// The gateway did not answer before the timeout. It may or may not have gone out, so it is
    /// never retried automatically — a retry could send the recipient a second copy.
    /// </summary>
    Undetermined,
}

public enum SmsAnalyticsGranularity { Day, Week, Month }
