using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;

namespace OrgHub.Api.Areas.FixedAssets.Controllers;

/// <summary>
///  This Controller is used to manage equipments.
///  </summary>
[Route("api/[area]/[controller]")]
[ApiController]
[Area("FixedAssets")]
public class EquipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// this constructor is used to initialize the EquipmentsController class.
    /// </summary>
    public EquipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// This method is used to get all equipments.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EquipmentDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllEquipmentQuery());
        return Ok(result);
    }

    /// <summary>
    /// this method is used to get an equipment by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetEquipmentByIdQuery(id));
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// this method is used to create an equipment.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EquipmentDto>> Create([FromBody] CreateEquipmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// this method is used to update an equipment by id.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EquipmentDto>> Update(int id, [FromBody] UpdateEquipmentCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        var result = await _mediator.Send(command);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// this method is used to delete an equipment by id.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteEquipmentCommand(id));
        return NoContent();
    }
}
