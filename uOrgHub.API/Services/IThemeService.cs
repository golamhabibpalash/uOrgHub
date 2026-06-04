using uOrgHub.API.DTOs;

namespace uOrgHub.API.Services;

public interface IThemeService
{
    Task<ThemeSettingsDto> GetCurrentAsync();
    Task<ThemeSettingsDto> UpdateAsync(UpdateThemeDto dto);
    Task<ThemeSettingsDto> ResetDefaultAsync();
}
