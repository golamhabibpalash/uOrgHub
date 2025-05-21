using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.Identity.Commands;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Queries;

namespace OrgHub.Api.Areas.Identity.Controllers;

/// <summary>
/// Controller for managing user permissions in the Identity area.
/// </summary>
[Route("api/[area]/[controller]")]
[ApiController]
[Area("Identity")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance for handling requests.</param>
    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Assigns permissions to a user.
    /// </summary>
    /// <param name="command">The command containing user and permission details.</param>
    /// <returns>The updated user permissions.</returns>
    [HttpPost("assign")]
    public async Task<ActionResult<UserPermissionsDto>> AssignPermissions([FromBody] AssignPermissionToUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Removes permissions from a user.
    /// </summary>
    /// <param name="command">The command containing user and permission details.</param>
    /// <returns>The updated user permissions.</returns>
    [HttpPost("remove")]
    public async Task<ActionResult<UserPermissionsDto>> RemovePermissions([FromBody] RemovePermissionFromUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The permissions assigned to the user.</returns>
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions(int userId)
    {
        var result = await _mediator.Send(new GetPermissionsForUserQuery());
        return Ok(result);
    }

    /// <summary>
    /// Gets all available permissions in the system.
    /// </summary>
    /// <returns>A list of all available permissions.</returns>
    [HttpGet("all")]
    public async Task<ActionResult<List<string>>> GetAllPermissions()
    {
        var result = await _mediator.Send(new GetAllPermissionsQuery());
        return Ok(result);
    }
}

