namespace OrgHub.Domain.Entities
{
    public abstract class CommonProps
    {
        public required DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid CreatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public Guid? LastUpdatedBy { get; set; }
    }
}
