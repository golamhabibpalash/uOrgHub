using uOrgHub.API.DTOs;
using uOrgHub.HR.DTOs;

namespace uOrgHub.API.Services;

public interface IEmployeeWithUserService
{
    Task<EmployeeResponseDto> CreateEmployeeWithUserAsync(CreateEmployeeWithUserDto dto);
}
