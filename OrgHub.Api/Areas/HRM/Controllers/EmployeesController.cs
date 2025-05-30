using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.HRM.Employees.Commands;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using System.Threading.Tasks;

namespace OrgHub.Api.Areas.HRM.Controllers;


/// <summary>
/// This class is used to manage employees.
/// </summary>
[ApiController]
[Route("api/[area]/[controller]")]
[Area("HRM")]
[Authorize]
public class EmployeesController : ControllerBase
{
    #region Fields
    private readonly IMediator _mediator;
    #endregion

    #region Ctor

    /// <summary>
    /// This constructor is used to initialize the EmployeesController class.
    /// </summary>
    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region Create
    /// <summary>
    /// This method is used to create an employee Post.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateEmployeeCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    #endregion

    #region Read

    /// <summary>
    /// This method is used to get an employee By Id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetByIdCommand(id));
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// This method is used to get all employees Search by Name/HRM_Designation/Phone/Email etc
    /// </summary>
    [HttpGet("getByInfo")]
    public async Task<IActionResult> GetByInfo(string info)
    {
        var result = await _mediator.Send(new GetByInfoCommand(info));
        return Ok(result);
    }


    /// <summary>
    /// This method is used to get all employees Post.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllEmployeesCommand());
        return Ok(result);
    }
    #endregion

    #region Update

    /// <summary>
    /// This method is used to update an employee.
    /// </summary>

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeCommand request)
    {
        var result =await _mediator.Send(request);
        return Ok(result);
    }
    #endregion

    #region Delete
    /// <summary>
    /// This method is used to delete an employee.
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result =await _mediator.Send(new DeleteEmployeeCommand(id));
        return Ok(result);
    }
    #endregion

    #region Locals
    /// <summary>
    /// Handles a test request to verify authorization functionality.
    /// </summary>
    /// <remarks>This endpoint requires authorization and is intended for testing purposes.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing a success message if authorization is successful.</returns>
    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Authorization worked!");
    }
    #endregion

}