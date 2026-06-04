using uOrgHub.API.DTOs;

namespace uOrgHub.API.Services;

public interface IHRDashboardService
{
    Task<HRDashboardDto> GetAsync();
}
