using System.ComponentModel.DataAnnotations;

namespace OrgHub.Domain.Entities.HRM;

public class Designation : CommonProps
{
    [Key]
    public int Id { get; set; }
    [StringLength(50)]
    public required string Title { get; set; }
    public int ParentDesignationId { get; set; }
    public int DesignationLevel { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Employee>? Employees { get; set; }
}
