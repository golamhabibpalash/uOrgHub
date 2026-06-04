using Microsoft.EntityFrameworkCore;
using uOrgHub.API.DTOs;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;

namespace uOrgHub.API.Services;

public class ThemeService : IThemeService
{
    private readonly AppDbContext _db;

    public ThemeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ThemeSettingsDto> GetCurrentAsync()
    {
        var theme = await _db.Set<ThemeSettings>().FirstOrDefaultAsync(t => !t.IsDefault);
        if (theme is null)
        {
            _db.Set<ThemeSettings>().Add(new ThemeSettings { IsDefault = false });
            await _db.SaveChangesAsync();
            theme = await _db.Set<ThemeSettings>().FirstAsync(t => !t.IsDefault);
        }
        return Map(theme);
    }

    public async Task<ThemeSettingsDto> UpdateAsync(UpdateThemeDto dto)
    {
        var theme = await _db.Set<ThemeSettings>().FirstOrDefaultAsync(t => !t.IsDefault)
                    ?? new ThemeSettings();

        theme.ThemeName = dto.ThemeName;
        theme.PrimaryColor = dto.PrimaryColor;
        theme.SidebarBg = dto.SidebarBg;
        theme.SidebarText = dto.SidebarText;
        theme.SidebarActiveBg = dto.SidebarActiveBg;
        theme.SidebarActiveText = dto.SidebarActiveText;
        theme.IsDarkMode = dto.IsDarkMode;
        theme.IsDefault = false;
        theme.UpdatedAt = DateTime.UtcNow;

        if (_db.Entry(theme).State == EntityState.Detached)
            _db.Set<ThemeSettings>().Add(theme);

        await _db.SaveChangesAsync();
        return Map(theme);
    }

    public async Task<ThemeSettingsDto> ResetDefaultAsync()
    {
        var existing = await _db.Set<ThemeSettings>().Where(t => !t.IsDefault).ToListAsync();
        _db.Set<ThemeSettings>().RemoveRange(existing);

        var defaults = new ThemeSettings { IsDefault = false };
        _db.Set<ThemeSettings>().Add(defaults);
        await _db.SaveChangesAsync();
        return Map(defaults);
    }

    private static ThemeSettingsDto Map(ThemeSettings t) => new(
        t.Id, t.ThemeName, t.PrimaryColor, t.SidebarBg, t.SidebarText,
        t.SidebarActiveBg, t.SidebarActiveText, t.IsDarkMode
    );
}
