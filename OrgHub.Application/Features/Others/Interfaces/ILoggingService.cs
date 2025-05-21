namespace OrgHub.Application.Features.Others.Interfaces;

public interface ILoggingService
{
    void LogActivity(string action, string message, Guid? userId = null);
    void LogAudit(string entity, Guid entityId, string action, string changes, Guid? userId = null);
}
