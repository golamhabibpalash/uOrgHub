using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace uOrgHub.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected string GetUserName() => User.FindFirstValue("username") ?? "system";
    protected Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
