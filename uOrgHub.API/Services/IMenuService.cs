using uOrgHub.Shared.Models;

namespace uOrgHub.API.Services;

public interface IMenuService
{
    List<MenuItemDto> GetAuthorizedMenu(List<string> userClaims, List<string> userRoles);
}
