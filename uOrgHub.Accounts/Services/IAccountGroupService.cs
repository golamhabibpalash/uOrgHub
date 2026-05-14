using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Services;

public interface IAccountGroupService : IBaseService<AccountGroupResponseDto, CreateAccountGroupDto, UpdateAccountGroupDto>
{
}