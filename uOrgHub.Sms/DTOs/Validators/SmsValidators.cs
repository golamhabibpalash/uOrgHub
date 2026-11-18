using FluentValidation;

namespace uOrgHub.Sms.DTOs.Validators;

public class CreateSmsGatewayDtoValidator : AbstractValidator<CreateSmsGatewayDto>
{
    public CreateSmsGatewayDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProviderKey).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SenderId).MaximumLength(50);
        RuleFor(x => x.BaseUrl).MaximumLength(500).Must(SmsGatewayRules.BeHttpUrl).When(x => !string.IsNullOrWhiteSpace(x.BaseUrl))
            .WithMessage("Base URL must be an http(s) URL.");
        RuleFor(x => x.RatePerSegment).GreaterThanOrEqualTo(0).When(x => x.RatePerSegment.HasValue);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.TimeoutSeconds).InclusiveBetween(5, 120);
        RuleFor(x => x.MaxSendsPerSecond).InclusiveBetween(1, 100);
    }
}

public class UpdateSmsGatewayDtoValidator : AbstractValidator<UpdateSmsGatewayDto>
{
    public UpdateSmsGatewayDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SenderId).MaximumLength(50);
        RuleFor(x => x.BaseUrl).MaximumLength(500).Must(SmsGatewayRules.BeHttpUrl).When(x => !string.IsNullOrWhiteSpace(x.BaseUrl))
            .WithMessage("Base URL must be an http(s) URL.");
        RuleFor(x => x.RatePerSegment).GreaterThanOrEqualTo(0).When(x => x.RatePerSegment.HasValue);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.TimeoutSeconds).InclusiveBetween(5, 120);
        RuleFor(x => x.MaxSendsPerSecond).InclusiveBetween(1, 100);
    }
}

public class SendSmsDtoValidator : AbstractValidator<SendSmsDto>
{
    public SendSmsDtoValidator()
    {
        RuleFor(x => x.Recipients).NotEmpty().WithMessage("Add at least one recipient.");
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message cannot be empty.")
            .MaximumLength(SmsLimits.MaxMessageLength)
            .WithMessage($"Message cannot exceed {SmsLimits.MaxMessageLength} characters.");
        RuleFor(x => x.IdempotencyKey).NotEmpty().Length(8, 100);
    }
}

public static class SmsGatewayRules
{
    public static bool BeHttpUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public static class SmsLimits
{
    /// <summary>Recipients in one send. Larger lists should be split.</summary>
    public const int MaxRecipients = 1000;
    /// <summary>About 10 Unicode / 25 GSM segments — far past what any provider will concatenate.</summary>
    public const int MaxMessageLength = 1600;
    /// <summary>A template is filled in before sending, so it leaves room for the values.</summary>
    public const int MaxTemplateLength = 1000;
}

public class UpdateSmsTemplateDtoValidator : AbstractValidator<UpdateSmsTemplateDto>
{
    public UpdateSmsTemplateDtoValidator()
    {
        RuleFor(x => x.Body).NotEmpty().WithMessage("Template text cannot be empty.")
            .MaximumLength(SmsLimits.MaxTemplateLength)
            .WithMessage($"Template cannot exceed {SmsLimits.MaxTemplateLength} characters.");
    }
}
