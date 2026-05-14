namespace uOrgHub.Auth.DTOs;

public record UserSessionDto(
    Guid Id,
    Guid UserId,
    string? DeviceInfo,
    string? IpAddress,
    string? Browser,
    string? Os,
    DateTime LoginAt,
    DateTime LastActivityAt,
    DateTime? LogoutAt,
    bool IsActive,
    string? LogoutReason
);

public record UserAccessLogDto(
    Guid Id,
    Guid? UserId,
    string? Username,
    string Action,
    string? Module,
    string? EntityType,
    string? EntityId,
    string? HttpMethod,
    string? Endpoint,
    int ResponseStatusCode,
    string? IpAddress,
    string? UserAgent,
    long DurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    string? OldValues,
    string? NewValues,
    DateTime CreatedAt
);
