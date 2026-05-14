using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_clients")]
public class Client : BaseEntity
{
    [Required][MaxLength(20)]  public string ClientCode { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string CompanyName { get; set; } = string.Empty;
    [MaxLength(100)]           public string? ContactPerson { get; set; }
    [MaxLength(200)]           public string? Email { get; set; }
    [MaxLength(20)]            public string? Phone { get; set; }
    [MaxLength(500)]           public string? Address { get; set; }
    public ClientType ClientType { get; set; }
    public ClientStatus Status { get; set; } = ClientStatus.Active;
    [MaxLength(1000)]          public string? Notes { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
