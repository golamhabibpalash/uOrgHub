using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Services;
using uOrgHub.Sms.Models.Entities;

namespace uOrgHub.Sms.Services;

/// <summary>Renders an event with the current company's template (or the built-in default) and its name as {company}.</summary>
public sealed class SmsTemplateService(AppDbContext db, ICurrentCompanyAccessor company) : ISmsTemplateService
{
    public async Task<string> RenderAsync(string eventKey, IReadOnlyDictionary<string, string> values, CancellationToken ct = default)
    {
        var info = SmsTemplateCatalog.Find(eventKey) ?? throw new AppException($"Unknown SMS event '{eventKey}'.");
        var custom = await db.Set<SmsTemplate>().AsNoTracking()
            .Where(t => !t.IsDeleted && t.EventKey == info.Key)
            .Select(t => t.Body)
            .FirstOrDefaultAsync(ct);

        var all = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        if (!all.ContainsKey(SmsTemplatePlaceholders.Company))
            all[SmsTemplatePlaceholders.Company] = await CompanyNameAsync(ct);

        return SmsTemplateCatalog.Render(custom ?? info.DefaultBody, all);
    }

    private async Task<string> CompanyNameAsync(CancellationToken ct)
    {
        if (company.CompanyId is not { } id) return string.Empty;
        return await db.Set<Company>().AsNoTracking().Where(c => c.Id == id).Select(c => c.Name).FirstOrDefaultAsync(ct) ?? string.Empty;
    }
}
