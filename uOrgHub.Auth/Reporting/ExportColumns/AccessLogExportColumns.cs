using uOrgHub.Auth.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Auth.Reporting.ExportColumns;

public static class AccessLogExportColumns
{
    public static List<ExportColumn<UserAccessLogDto>> Get() =>
    [
        new("username", "Username", x => x.Username),
        new("action", "Action", x => x.Action),
        new("module", "Module", x => x.Module),
        new("entityType", "Entity Type", x => x.EntityType),
        new("entityId", "Entity ID", x => x.EntityId),
        new("httpMethod", "HTTP Method", x => x.HttpMethod),
        new("endpoint", "Endpoint", x => x.Endpoint),
        new("statusCode", "Status Code", x => x.ResponseStatusCode),
        new("ipAddress", "IP Address", x => x.IpAddress),
        new("durationMs", "Duration (ms)", x => x.DurationMs),
        new("isSuccess", "Is Success", x => x.IsSuccess),
        new("errorMessage", "Error Message", x => x.ErrorMessage),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
