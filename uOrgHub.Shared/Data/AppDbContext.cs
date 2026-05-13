using Microsoft.EntityFrameworkCore;

namespace uOrgHub.Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Discover and apply entity configurations from all uOrgHub module assemblies loaded at runtime
        var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("uOrgHub.") == true);

        foreach (var assembly in moduleAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
