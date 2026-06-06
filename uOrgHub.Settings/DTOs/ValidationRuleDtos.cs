namespace uOrgHub.Settings.DTOs;

public record CreateValidationRuleDto(
    string EntityType,
    string FieldName,
    string RuleType,
    string? RuleValue,
    string? ErrorMessage,
    string Severity = "Error",
    bool IsEnabled = true,
    int SortOrder = 0
);

public record UpdateValidationRuleDto(
    string EntityType,
    string FieldName,
    string RuleType,
    string? RuleValue,
    string? ErrorMessage,
    string Severity,
    bool IsEnabled,
    int SortOrder
);

public record ValidationRuleResponseDto(
    Guid Id,
    string EntityType,
    string FieldName,
    string RuleType,
    string? RuleValue,
    string? ErrorMessage,
    string Severity,
    bool IsEnabled,
    int SortOrder,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);
