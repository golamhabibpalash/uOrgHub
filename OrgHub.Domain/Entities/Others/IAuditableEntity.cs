namespace OrgHub.Domain.Entities.Others;

public interface IAuditableEntity
{
    Guid? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    Guid? LastUpdatedBy { get; set; }
    DateTime? LastUpdatedAt { get; set; }
}

