using uOrgHub.Shared.Services;

namespace uOrgHub.Sms.Services;

public record SmsPlaceholderInfo(string Key, string Description, string Sample);

public record SmsTemplateEventInfo(
    string Key, string Label, string Description, string DefaultBody, IReadOnlyList<SmsPlaceholderInfo> Placeholders);

/// <summary>
/// The message events modules raise, each with a built-in default text. A company edits its own
/// copy under SMS → Templates; until then the default is used, so nothing needs seeding. Adding an
/// event: a key in <see cref="SmsTemplateEvents"/>, an entry here, and the module that raises it.
/// </summary>
public static class SmsTemplateCatalog
{
    private static readonly IReadOnlyList<SmsPlaceholderInfo> MoneyPlaceholders =
    [
        new(SmsTemplatePlaceholders.Party, "Customer, vendor or client name", "Rahim Traders"),
        new(SmsTemplatePlaceholders.Amount, "Amount, e.g. 25,000.00", "25,000.00"),
        new(SmsTemplatePlaceholders.Date, "Payment / voucher date (dd/MM/yyyy)", "18/11/2026"),
        new(SmsTemplatePlaceholders.Reference, "MR No., payment or voucher number", "MR-00125"),
        new(SmsTemplatePlaceholders.Method, "Payment method or cash/bank account", "Bank Transfer"),
        new(SmsTemplatePlaceholders.Company, "Your company name", "Your Company Ltd"),
    ];

    public static IReadOnlyList<SmsTemplateEventInfo> Events { get; } =
    [
        new(SmsTemplateEvents.MoneyReceived, "Money received",
            "Sent to a customer, client or vendor after money is received from them — Payments (received) and Credit vouchers.",
            "Dear {party}, we have received BDT {amount} on {date} (Ref: {reference}). Thank you. - {company}",
            MoneyPlaceholders),
        new(SmsTemplateEvents.MoneyPaid, "Money paid",
            "Sent to a vendor, customer or client after a payment is made to them — Payments (paid out) and Debit vouchers.",
            "Dear {party}, we have paid you BDT {amount} on {date} (Ref: {reference}). - {company}",
            MoneyPlaceholders),
    ];

    public static SmsTemplateEventInfo? Find(string eventKey)
        => Events.FirstOrDefault(e => string.Equals(e.Key, eventKey, StringComparison.OrdinalIgnoreCase));

    /// <summary>Replaces each {placeholder} with its value (case-insensitive); unknown placeholders are left as typed.</summary>
    public static string Render(string body, IReadOnlyDictionary<string, string> values)
    {
        var result = body;
        foreach (var (key, value) in values)
            result = result.Replace("{" + key + "}", value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        return result.Trim();
    }
}
