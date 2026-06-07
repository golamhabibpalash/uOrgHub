using System.Text.RegularExpressions;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Validation;

/// <summary>
/// Shared email / mobile-number rules for employees. Used both by the FluentValidation
/// DTO validators and enforced at runtime in the command handlers / services, since the
/// MediatR validation pipeline validates commands (not the nested DTOs).
/// </summary>
public static partial class EmployeeContactRules
{
    /// <summary>
    /// Bangladeshi mobile number: optional +88 / 88 prefix, then 01, an operator digit (3-9)
    /// and 8 more digits — e.g. 01712345678 or +8801712345678.
    /// </summary>
    public const string MobilePattern = @"^(?:\+?88)?01[3-9]\d{8}$";

    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public const string MobileMessage = "Please enter a valid mobile number (e.g. 01712345678).";
    public const string EmailMessage = "Please enter a valid email address.";

    [GeneratedRegex(MobilePattern)]
    private static partial Regex MobileRegex();

    [GeneratedRegex(EmailPattern)]
    private static partial Regex EmailRegex();

    public static bool IsValidEmail(string? value) =>
        !string.IsNullOrWhiteSpace(value) && EmailRegex().IsMatch(value.Trim());

    /// <summary>Empty / null is treated as valid here — emptiness is enforced separately where required.</summary>
    public static bool IsValidMobile(string? value) =>
        string.IsNullOrWhiteSpace(value) || MobileRegex().IsMatch(value.Trim());

    /// <summary>Throws <see cref="AppException"/> (400) with a user-friendly message on the first failure.</summary>
    public static void Validate(string email, string? phone, string? mobilePhone = null)
    {
        if (!IsValidEmail(email))
            throw new AppException(EmailMessage);
        if (!IsValidMobile(phone) || !IsValidMobile(mobilePhone))
            throw new AppException(MobileMessage);
    }
}
