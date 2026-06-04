namespace uOrgHub.Shared.Models;

public record MenuItemDto(
    string Key,
    string Label,
    string? Icon,
    string? Path,
    string? RequiredClaim,
    string? RequiredRole,
    string? Section,
    List<MenuItemDto>? Children
);
