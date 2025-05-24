using Microsoft.AspNetCore.Http;
using OrgHub.Application.Features.Others.Interfaces;
using Serilog;
using System.Security.Claims;

namespace OrgHub.Application.Features.Others.Services;

public class LoggingService : ILoggingService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public void LogActivity(string action, string message, Guid? userId = null)
    {
        userId ??= GetCurrentUserId();
        Log.Information("Activity: {Action}, Message: {Message}, UserId: {UserId}", action, message, userId);
    }
    public void LogAudit(string entity, Guid entityId, string action, string changes, Guid? userId = null)
    {
        userId ??= GetCurrentUserId();
        Log.Information($"Audit: {entity}, EntityId: {entityId}, Action: {action}, Changes: {changes}, UserId: {userId}");
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }
}
