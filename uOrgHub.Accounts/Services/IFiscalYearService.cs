using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Services;

public interface IFiscalYearService : IBaseService<FiscalYearResponseDto, CreateFiscalYearDto, UpdateFiscalYearDto>
{
    Task<FiscalYearResponseDto> GetCurrentAsync();
}