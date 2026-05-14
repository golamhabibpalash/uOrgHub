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

        var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("uOrgHub.") == true);

        foreach (var assembly in moduleAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
