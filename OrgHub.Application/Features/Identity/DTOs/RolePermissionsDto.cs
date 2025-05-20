namespace OrgHub.Application.Features.Identity.DTOs;

public class RolePermissionsDto
{
    public int RoleId { get; set; }
    public IList<string> Permissions { get; set; } = new List<string>();
}
