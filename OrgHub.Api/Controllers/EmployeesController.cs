using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgHub.Application.Features.Employees.Commands;

namespace OrgHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("OrgHub API is working ✅");
    }

    public async Task<ActionResult> Create([FromBody] CreateEmployeeCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}