namespace OrgHub.Application.Features.Identity.DTOs;

public class UserPermissionsDto
{
    public int UserId { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}
