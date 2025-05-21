namespace OrgHub.Domain.Entities.Identity;

public interface ICommonProps
{
    DateTime CreatedDate { get; set; }
    int CreatedBy { get; set; }
    DateTime? LastUpdateDate { get; set; }
    int LastUpdatedBy { get; set; }
}
