namespace OrgHub.Domain.Entities.Identity;

public interface ICommonProps
{
    DateTime CreatedDate { get; set; }
    Guid CreatedBy { get; set; }
    DateTime? LastUpdateDate { get; set; }
    Guid? LastUpdatedBy { get; set; }
}
