using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Dashboard.Queries;
using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.API.Controllers.Sms;

[Authorize]
public class SmsDashboardController(IMediator mediator) : BaseController
{
    [HttpGet]
    [RequireClaim(Claims.Sms.Dashboard.View)]
    public async Task<IActionResult> Get([FromQuery] int tzOffsetMinutes = 0)
        => Ok(ApiResponse<SmsDashboardDto>.Ok(await mediator.Send(new GetSmsDashboardQuery(tzOffsetMinutes))));

    /// <param name="from">First local day (yyyy-MM-dd).</param>
    /// <param name="to">Last local day, inclusive.</param>
    [HttpGet("analytics")]
    [RequireClaim(Claims.Sms.Analytics.View)]
    public async Task<IActionResult> Analytics(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] SmsAnalyticsGranularity granularity = SmsAnalyticsGranularity.Day,
        [FromQuery] int tzOffsetMinutes = 0, [FromQuery] Guid? gatewayId = null)
        => Ok(ApiResponse<SmsAnalyticsDto>.Ok(
            await mediator.Send(new GetSmsAnalyticsQuery(from, to, granularity, tzOffsetMinutes, gatewayId))));
}
