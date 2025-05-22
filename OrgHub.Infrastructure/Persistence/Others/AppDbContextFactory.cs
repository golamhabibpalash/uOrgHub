using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrgHub.Infrastructure.Persistence.Others;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // points to the OrgHub.Api project if running from root
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseSqlServer(connectionString);

        // Create an instance of AuditLoggingInterceptor (or retrieve it from a service provider if needed)
        var auditLoggingInterceptor = new AuditLoggingInterceptor();

        return new AppDbContext(builder.Options, auditLoggingInterceptor);
    }
}
