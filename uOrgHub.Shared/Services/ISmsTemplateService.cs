namespace uOrgHub.Shared.Services;

/// <summary>
/// Message events a module can raise an SMS for. Each has one template per company, edited under
/// SMS → Templates (built-in default until then).
/// </summary>
public static class SmsTemplateEvents
{
    public const string MoneyReceived = "MoneyReceived";
    public const string MoneyPaid = "MoneyPaid";
}

/// <summary>
/// Placeholder names a template can use. Callers supply the values for an event; {company} is
/// filled in by the template service from the current company.
/// </summary>
public static class SmsTemplatePlaceholders
{
    public const string Party = "party";
    public const string Amount = "amount";
    public const string Date = "date";
    public const string Reference = "reference";
    public const string Method = "method";
    public const string Company = "company";
}

/// <summary>Builds an event's message from the current company's template (implemented in uOrgHub.Sms).</summary>
public interface ISmsTemplateService
{
    Task<string> RenderAsync(string eventKey, IReadOnlyDictionary<string, string> values, CancellationToken ct = default);
}
