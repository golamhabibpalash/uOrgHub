using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.Clients.Commands;
using uOrgHub.Projects.Features.Clients.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ClientsController : BaseController
{
    private readonly IMediator _mediator;
    public ClientsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.Clients.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] ClientStatus? status = null)
    {
        var result = await _mediator.Send(new GetClientsQuery(request, status));
        return Ok(ApiResponse<PagedResult<ClientResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Clients.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetClientByIdQuery(id));
        return Ok(ApiResponse<ClientResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Clients.Create)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
    {
        var result = await _mediator.Send(new CreateClientCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ClientResponseDto>.Ok(result, "Client created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Clients.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientDto dto)
    {
        var result = await _mediator.Send(new UpdateClientCommand(id, dto));
        return Ok(ApiResponse<ClientResponseDto>.Ok(result, "Client updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Clients.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteClientCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Client deleted successfully."));
    }
}
