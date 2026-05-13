using uOrgHub.HR.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.HR.Services;

public interface IDepartmentService : IBaseService<DepartmentResponseDto, CreateDepartmentDto, UpdateDepartmentDto>
{
}
