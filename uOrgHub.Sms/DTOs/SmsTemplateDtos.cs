using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.DTOs;

public class SmsTemplateDto
{
    public string EventKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>The text in use: the company's own, or the default when <see cref="IsCustom"/> is false.</summary>
    public string Body { get; set; } = string.Empty;
    public string DefaultBody { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public IReadOnlyList<SmsPlaceholderInfo> Placeholders { get; set; } = [];
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateSmsTemplateDto
{
    public string Body { get; set; } = string.Empty;
}
