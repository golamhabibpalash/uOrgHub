using Microsoft.AspNetCore.Mvc;

namespace uOrgHub.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
}
