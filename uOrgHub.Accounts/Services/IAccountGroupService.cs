using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Services;

public interface IAccountGroupService : IBaseService<AccountGroupResponseDto, CreateAccountGroupDto, UpdateAccountGroupDto>
{
    Task<List<AccountGroupResponseDto>> GetAllFlatAsync();
    Task<string> GenerateCodeAsync(Models.Enums.AccountGroupType type, Guid? parentId);
}