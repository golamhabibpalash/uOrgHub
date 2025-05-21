namespace OrgHub.Domain.Entities
{
    public abstract class CommonProps
    {
        public required DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int LastUpdatedBy { get; set; } = 0;
    }
}
