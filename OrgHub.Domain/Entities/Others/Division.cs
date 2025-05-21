namespace OrgHub.Domain.Entities.Others;

public class Division : CommonProps
{
    public int Id { get; set; }
    public required string Name { get; set; } = default!;
    // Navigation property for related District entities
    public virtual ICollection<District> Districts { get; set; } = new HashSet<District>();
}
