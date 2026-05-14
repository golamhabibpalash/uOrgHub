namespace uOrgHub.Auth.DTOs;

public record AccessLogDto(
    Guid Id,
    Guid? UserId,
    string? Username,
    string Action,
    string? Module,
    string? Endpoint,
    string? HttpMethod,
    int ResponseStatusCode,
    string? IpAddress,
    long DurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    DateTime CreatedAt
);

public record AccessLogFilterRequest(
    int Page = 1,
    int PageSize = 20,
    Guid? UserId = null,
    string? Module = null,
    string? Action = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    bool? IsSuccess = null,
    string? HttpMethod = null
);

public record AccessLogSummaryDto(
    long TotalRequests,
    long FailedRequests,
    double FailureRate,
    double AvgDurationMs,
    int UniqueUsersToday,
    int ActiveSessionsNow
);
