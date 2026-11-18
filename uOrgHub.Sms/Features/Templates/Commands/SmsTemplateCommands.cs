using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Features.Templates.Queries;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Templates.Commands;

public record UpdateSmsTemplateCommand(string EventKey, UpdateSmsTemplateDto Dto) : ICommand<SmsTemplateDto>;
/// <summary>Drops the company's text so the built-in default applies again.</summary>
public record ResetSmsTemplateCommand(string EventKey) : ICommand<SmsTemplateDto>;

public partial class UpdateSmsTemplateCommandHandler(AppDbContext db) : IRequestHandler<UpdateSmsTemplateCommand, SmsTemplateDto>
{
    [GeneratedRegex(@"\{([A-Za-z_]+)\}")]
    private static partial Regex Placeholder();

    public async Task<SmsTemplateDto> Handle(UpdateSmsTemplateCommand request, CancellationToken ct)
    {
        var info = SmsTemplateCatalog.Find(request.EventKey)
            ?? throw new AppException($"Unknown SMS event '{request.EventKey}'.", 404);
        var body = request.Dto.Body.Trim();

        // A mistyped placeholder would go out literally as "{amout}" — refuse it here instead.
        var known = info.Placeholders.Select(p => p.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknown = Placeholder().Matches(body).Select(m => m.Groups[1].Value)
            .Where(k => !known.Contains(k)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (unknown.Count > 0)
            throw new ValidationException(unknown.Select(k =>
                $"{{{k}}} is not a placeholder. Use one of: {string.Join(", ", known.Select(x => "{" + x + "}"))}.").ToList());

        var row = await db.Set<SmsTemplate>().FirstOrDefaultAsync(t => !t.IsDeleted && t.EventKey == info.Key, ct);
        if (row is null)
        {
            row = new SmsTemplate { EventKey = info.Key, Body = body };
            db.Set<SmsTemplate>().Add(row);
        }
        else
        {
            row.Body = body;
            row.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return SmsTemplateViews.ToDto(info, row);
    }
}

public class ResetSmsTemplateCommandHandler(AppDbContext db) : IRequestHandler<ResetSmsTemplateCommand, SmsTemplateDto>
{
    public async Task<SmsTemplateDto> Handle(ResetSmsTemplateCommand request, CancellationToken ct)
    {
        var info = SmsTemplateCatalog.Find(request.EventKey)
            ?? throw new AppException($"Unknown SMS event '{request.EventKey}'.", 404);
        var row = await db.Set<SmsTemplate>().FirstOrDefaultAsync(t => !t.IsDeleted && t.EventKey == info.Key, ct);
        if (row is not null)
        {
            row.IsDeleted = true;
            row.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return SmsTemplateViews.ToDto(info, null);
    }
}
