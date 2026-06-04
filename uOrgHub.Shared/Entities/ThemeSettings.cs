namespace uOrgHub.Shared.Entities;

public class ThemeSettings : BaseEntity
{
    public string ThemeName { get; set; } = "corporate-blue";
    public string PrimaryColor { get; set; } = "#0ea5e9";
    public string SidebarBg { get; set; } = "#1e293b";
    public string SidebarText { get; set; } = "#cbd5e1";
    public string SidebarActiveBg { get; set; } = "#0ea5e9";
    public string SidebarActiveText { get; set; } = "#ffffff";
    public bool IsDarkMode { get; set; }
    public bool IsDefault { get; set; }
}
