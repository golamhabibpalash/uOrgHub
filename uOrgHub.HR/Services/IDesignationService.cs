using uOrgHub.HR.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.HR.Services;

public interface IDesignationService : IBaseService<DesignationResponseDto, CreateDesignationDto, UpdateDesignationDto>
{
}
