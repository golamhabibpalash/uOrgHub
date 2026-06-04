namespace uOrgHub.Auth.Seeders;

public interface IAuthSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}
