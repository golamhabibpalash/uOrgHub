using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/customers")]
public class CustomersController : BaseController
{
    private readonly IMediator _mediator;
    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Accounts.Customers.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetCustomersQuery(request));
        return Ok(ApiResponse<PagedResult<CustomerResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Customers.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(ApiResponse<CustomerResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Customers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CustomerResponseDto>.Ok(result, "Customer created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.Customers.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(id, dto));
        return Ok(ApiResponse<CustomerResponseDto>.Ok(result, "Customer updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.Customers.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Customer deleted successfully."));
    }
}
