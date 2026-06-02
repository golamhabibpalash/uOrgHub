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
    string? Search = null,
    Guid? UserId = null,
    string? Username = null,
    string? Module = null,
    string? Action = null,
    string? HttpMethod = null,
    bool? IsSuccess = null,
    string? EntityType = null,
    string? IpAddress = null,
    int? StatusCodeFrom = null,
    int? StatusCodeTo = null,
    long? DurationMin = null,
    long? DurationMax = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SortBy = null,
    bool SortDescending = true
);

public record AccessLogSummaryDto(
    long TotalRequests,
    long FailedRequests,
    double FailureRate,
    double AvgDurationMs,
    int UniqueUsersToday,
    int ActiveSessionsNow
);
