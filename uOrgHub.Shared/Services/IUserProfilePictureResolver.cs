namespace uOrgHub.Shared.Services;

public interface IUserProfilePictureResolver
{
    Task<string?> ResolveAsync(Guid employeeId);
    Task<Dictionary<Guid, string?>> ResolveBatchAsync(IEnumerable<Guid> employeeIds);
}
