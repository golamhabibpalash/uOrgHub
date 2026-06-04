namespace uOrgHub.API.DTOs;

public record ThemeSettingsDto(
    Guid Id,
    string ThemeName,
    string PrimaryColor,
    string SidebarBg,
    string SidebarText,
    string SidebarActiveBg,
    string SidebarActiveText,
    bool IsDarkMode
);

public record UpdateThemeDto(
    string ThemeName,
    string PrimaryColor,
    string SidebarBg,
    string SidebarText,
    string SidebarActiveBg,
    string SidebarActiveText,
    bool IsDarkMode
);
