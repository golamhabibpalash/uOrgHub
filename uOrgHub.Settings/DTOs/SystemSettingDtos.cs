namespace uOrgHub.Settings.DTOs;

public record CreateSystemSettingDto(
    string Category,
    string Key,
    string Value,
    string DataType,
    string? Description,
    bool IsActive = true,
    bool IsSystem = false
);

public record UpdateSystemSettingDto(
    string Category,
    string Key,
    string Value,
    string DataType,
    string? Description,
    bool IsActive
);

public record SystemSettingResponseDto(
    Guid Id,
    string Category,
    string Key,
    string Value,
    string DataType,
    string? Description,
    bool IsActive,
    bool IsSystem,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);
