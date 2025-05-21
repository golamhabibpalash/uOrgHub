namespace OrgHub.Domain.Entities.Others;

public class District : CommonProps
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public required string Name { get; set; } = default!;
    public virtual required Division Division { get; set; } = default!;
    // Navigation property for related Upazila entities
    public virtual ICollection<Upazila> Upazilas { get; set; } = new HashSet<Upazila>();
}