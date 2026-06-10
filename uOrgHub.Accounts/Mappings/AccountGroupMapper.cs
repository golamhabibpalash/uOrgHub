using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

[Mapper]
public partial class AccountGroupMapper
{
    public AccountGroupResponseDto ToDto(AccountGroup entity)
    {
        var dto = MapToDto(entity);
        dto.ParentGroupName = entity.ParentAccountGroup?.Name;
        return dto;
    }

    private partial AccountGroupResponseDto MapToDto(AccountGroup entity);

    public partial AccountGroup ToEntity(CreateAccountGroupDto dto);
    public partial void UpdateEntity(UpdateAccountGroupDto dto, AccountGroup entity);
}