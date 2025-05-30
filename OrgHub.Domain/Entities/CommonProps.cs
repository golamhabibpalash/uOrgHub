namespace OrgHub.Domain.Entities
{
    public abstract class CommonProps
    {
        public required DateTime CreatedAt { get; set; } = DateTime.Now;
        public Guid CreatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedBy { get; set; }
    }
}
