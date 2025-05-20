using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.Identity.Commands;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Queries;

namespace OrgHub.Api.Areas.Identity.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Assigns permissions to a user.
    /// </summary>
    [HttpPost("assign")]
    public async Task<ActionResult<UserPermissionsDto>> AssignPermissions([FromBody] AssignPermissionToUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Removes permissions from a user.
    /// </summary>
    [HttpPost("remove")]
    public async Task<ActionResult<UserPermissionsDto>> RemovePermissions([FromBody] RemovePermissionFromUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions(int userId)
    {
        var result = await _mediator.Send(new GetPermissionsForUserQuery());
        return Ok(result);
    }

    /// <summary>
    /// Gets all available permissions in the system.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<string>>> GetAllPermissions()
    {
        var result = await _mediator.Send(new GetAllPermissionsQuery());
        return Ok(result);
    }
}

