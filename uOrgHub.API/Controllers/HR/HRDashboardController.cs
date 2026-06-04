using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.DTOs;
using uOrgHub.API.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[ApiController]
[Route("api/v1/hr/dashboard")]
[Authorize]
public class HRDashboardController : ControllerBase
{
    private readonly IHRDashboardService _service;

    public HRDashboardController(IHRDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAsync();
        return Ok(ApiResponse<HRDashboardDto>.Ok(result));
    }
}
