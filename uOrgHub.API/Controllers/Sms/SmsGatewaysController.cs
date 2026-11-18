using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features.Gateways.Commands;
using uOrgHub.Sms.Features.Gateways.Queries;
using uOrgHub.Sms.Providers;

namespace uOrgHub.API.Controllers.Sms;

/// <summary>
/// SMS gateway configuration. Responses never carry credential values — only whether each one is
/// set. Configuration changes need Manage; changing the active gateway needs Switch.
/// </summary>
[Authorize]
public class SmsGatewaysController(IMediator mediator) : BaseController
{
    [HttpGet]
    [RequireClaim(Claims.Sms.Gateways.View)]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<SmsGatewayResponseDto>>.Ok(await mediator.Send(new GetSmsGatewaysQuery())));

    /// <summary>Available provider types and the settings each needs — drives the gateway form.</summary>
    [HttpGet("providers")]
    [RequireClaim(Claims.Sms.Gateways.View)]
    public async Task<IActionResult> GetProviders()
        => Ok(ApiResponse<IReadOnlyList<SmsProviderDescriptor>>.Ok(await mediator.Send(new GetSmsProvidersQuery())));

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Sms.Gateways.View)]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(ApiResponse<SmsGatewayResponseDto>.Ok(await mediator.Send(new GetSmsGatewayByIdQuery(id))));

    [HttpPost]
    [RequireClaim(Claims.Sms.Gateways.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateSmsGatewayDto dto)
    {
        var result = await mediator.Send(new CreateSmsGatewayCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SmsGatewayResponseDto>.Ok(result, result.IsDefault ? "Gateway created and set as active." : "Gateway created."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Sms.Gateways.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSmsGatewayDto dto)
        => Ok(ApiResponse<SmsGatewayResponseDto>.Ok(await mediator.Send(new UpdateSmsGatewayCommand(id, dto)), "Gateway updated."));

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Sms.Gateways.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteSmsGatewayCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Gateway removed."));
    }

    [HttpPost("{id:guid}/enable")]
    [RequireClaim(Claims.Sms.Gateways.Manage)]
    public async Task<IActionResult> Enable(Guid id)
        => Ok(ApiResponse<SmsGatewayResponseDto>.Ok(await mediator.Send(new SetSmsGatewayEnabledCommand(id, true)), "Gateway enabled."));

    [HttpPost("{id:guid}/disable")]
    [RequireClaim(Claims.Sms.Gateways.Manage)]
    public async Task<IActionResult> Disable(Guid id)
        => Ok(ApiResponse<SmsGatewayResponseDto>.Ok(await mediator.Send(new SetSmsGatewayEnabledCommand(id, false)), "Gateway disabled."));

    /// <summary>Makes this the active gateway every send goes through.</summary>
    [HttpPost("{id:guid}/activate")]
    [RequireClaim(Claims.Sms.Gateways.Switch)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await mediator.Send(new ActivateSmsGatewayCommand(id));
        return Ok(ApiResponse<SmsGatewayResponseDto>.Ok(result, $"{result.Name} is now the active SMS gateway."));
    }

    [HttpPost("{id:guid}/test")]
    [RequireClaim(Claims.Sms.Gateways.Manage)]
    public async Task<IActionResult> Test(Guid id, [FromBody] TestSmsGatewayDto? dto)
    {
        var result = await mediator.Send(new TestSmsGatewayCommand(id, dto ?? new TestSmsGatewayDto(), GetUserId()));
        // A failed check is a normal answer, not an error: the body says what went wrong, and the
        // page shows it. The envelope message stays "Success" so the client's generic success
        // toast doesn't announce a failed test in green.
        return Ok(ApiResponse<SmsGatewayTestResultDto>.Ok(result));
    }

    /// <summary>Refreshes the balance from the provider (null id = active gateway). Throttled unless force=true.</summary>
    [HttpPost("sync-balance")]
    [RequireClaim(Claims.Sms.Dashboard.View)]
    public async Task<IActionResult> SyncBalance([FromQuery] Guid? gatewayId, [FromQuery] bool force = false)
        => Ok(ApiResponse<SmsBalanceDto?>.Ok(await mediator.Send(new SyncSmsBalanceCommand(gatewayId, force))));
}
