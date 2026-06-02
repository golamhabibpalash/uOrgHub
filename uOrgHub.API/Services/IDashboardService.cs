using uOrgHub.API.DTOs;

namespace uOrgHub.API.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
