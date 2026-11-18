using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Templates.Queries;

/// <summary>Every message event with the company's text (or the default) and its placeholders.</summary>
public record GetSmsTemplatesQuery : IQuery<List<SmsTemplateDto>>;

public class GetSmsTemplatesQueryHandler(AppDbContext db) : IRequestHandler<GetSmsTemplatesQuery, List<SmsTemplateDto>>
{
    public async Task<List<SmsTemplateDto>> Handle(GetSmsTemplatesQuery request, CancellationToken ct)
    {
        var custom = await db.Set<SmsTemplate>().AsNoTracking().Where(t => !t.IsDeleted).ToListAsync(ct);
        return SmsTemplateCatalog.Events.Select(e =>
        {
            var row = custom.FirstOrDefault(t => t.EventKey == e.Key);
            return SmsTemplateViews.ToDto(e, row);
        }).ToList();
    }
}

internal static class SmsTemplateViews
{
    /// <summary>Joins a catalog event with the company's row — not an entity mapping, so not a Mapperly case.</summary>
    public static SmsTemplateDto ToDto(SmsTemplateEventInfo info, SmsTemplate? row) => new()
    {
        EventKey = info.Key,
        Label = info.Label,
        Description = info.Description,
        Body = row?.Body ?? info.DefaultBody,
        DefaultBody = info.DefaultBody,
        IsCustom = row is not null,
        Placeholders = info.Placeholders,
        UpdatedAt = row?.UpdatedAt ?? row?.CreatedAt,
        UpdatedBy = row is null ? null : row.UpdatedBy ?? row.CreatedBy,
    };
}
