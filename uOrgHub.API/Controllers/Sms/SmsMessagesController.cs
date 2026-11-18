using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Models;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Messages.Commands;
using uOrgHub.Sms.Features.Messages.Queries;
using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.API.Controllers.Sms;

[Authorize]
public class SmsMessagesController(IMediator mediator, IPermissionService permissions) : BaseController
{
    /// <summary>
    /// Queues an SMS through the active gateway. One recipient needs Send; more than one also
    /// needs SendBulk — the handler counts recipients after normalising, so it can't be dodged by
    /// formatting. Re-posting the same idempotency key returns the original send.
    /// </summary>
    [HttpPost("send")]
    [RequireClaim(Claims.Sms.Messages.Send)]
    public async Task<IActionResult> Send([FromBody] SendSmsDto dto)
    {
        var allowBulk = await permissions.HasClaimAsync(GetUserId(), Claims.Sms.Messages.SendBulk);
        var result = await mediator.Send(new SendSmsCommand(dto, GetUserId(), allowBulk));
        var message = result.Duplicate
            ? "This message was already submitted; showing the original send."
            : $"Queued {result.RecipientCount} SMS ({result.TotalSegments} segment{(result.TotalSegments == 1 ? "" : "s")}) via {result.GatewayName}.";
        return Ok(ApiResponse<SendSmsResultDto>.Ok(result, message));
    }

    [HttpGet("compose-info")]
    [RequireClaim(Claims.Sms.Messages.Send)]
    public async Task<IActionResult> ComposeInfo()
    {
        var canBulk = await permissions.HasClaimAsync(GetUserId(), Claims.Sms.Messages.SendBulk);
        return Ok(ApiResponse<SmsComposeInfoDto>.Ok(await mediator.Send(new GetSmsComposeInfoQuery(canBulk))));
    }

    [HttpGet("requests/{id:guid}")]
    [RequireClaim(Claims.Sms.Messages.Send)]
    public async Task<IActionResult> RequestStatus(Guid id)
        => Ok(ApiResponse<SmsSendRequestStatusDto>.Ok(await mediator.Send(new GetSmsSendRequestStatusQuery(id))));

    /// <param name="from">Inclusive, UTC.</param>
    /// <param name="to">Exclusive, UTC.</param>
    [HttpGet]
    [RequireClaim(Claims.Sms.History.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequest request, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] Guid? gatewayId, [FromQuery] SmsMessageStatus? status)
        => Ok(ApiResponse<PagedResult<SmsMessageListItemDto>>.Ok(
            await mediator.Send(new GetSmsMessagesQuery(request, from, to, gatewayId, status))));

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Sms.History.ViewDetails)]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(ApiResponse<SmsMessageDetailDto>.Ok(await mediator.Send(new GetSmsMessageByIdQuery(id))));
}
