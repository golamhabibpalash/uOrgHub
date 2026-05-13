using uOrgHub.HR.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.HR.Services;

public interface IEmployeeService : IBaseService<EmployeeResponseDto, CreateEmployeeDto, UpdateEmployeeDto>
{
}
