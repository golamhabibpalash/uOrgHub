using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Templates.Commands;
using uOrgHub.Sms.Features.Templates.Queries;

namespace uOrgHub.API.Controllers.Sms;

/// <summary>Per-company message templates for SMS events (money received / paid).</summary>
[Authorize]
public class SmsTemplatesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [RequireClaim(Claims.Sms.Templates.View)]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<SmsTemplateDto>>.Ok(await mediator.Send(new GetSmsTemplatesQuery())));

    [HttpPut("{eventKey}")]
    [RequireClaim(Claims.Sms.Templates.Manage)]
    public async Task<IActionResult> Update(string eventKey, [FromBody] UpdateSmsTemplateDto dto)
        => Ok(ApiResponse<SmsTemplateDto>.Ok(await mediator.Send(new UpdateSmsTemplateCommand(eventKey, dto)), "Template saved."));

    [HttpDelete("{eventKey}")]
    [RequireClaim(Claims.Sms.Templates.Manage)]
    public async Task<IActionResult> Reset(string eventKey)
        => Ok(ApiResponse<SmsTemplateDto>.Ok(await mediator.Send(new ResetSmsTemplateCommand(eventKey)), "Template reset to the default."));
}
