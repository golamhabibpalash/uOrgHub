using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

[Mapper]
public partial class AccountGroupMapper
{
    public partial AccountGroupResponseDto ToDto(AccountGroup entity);
    public partial AccountGroup ToEntity(CreateAccountGroupDto dto);
    public partial void UpdateEntity(UpdateAccountGroupDto dto, AccountGroup entity);
}