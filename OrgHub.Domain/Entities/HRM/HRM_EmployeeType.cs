namespace OrgHub.Domain.Entities.HRM;

public class HRM_EmployeeType : CommonProps
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
