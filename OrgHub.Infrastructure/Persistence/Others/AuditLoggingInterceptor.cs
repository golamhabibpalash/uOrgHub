using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrgHub.Domain.Entities.Others;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

namespace OrgHub.Infrastructure.Persistence.Others;

public class AuditLoggingInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public List<AuditLog> PendingLogs { get; } = new();

    public AuditLoggingInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context as AppDbContext;
        if (context == null) return new(result);

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
        {
            var log = new AuditLog
            {
                TableName = entry.Metadata.GetTableName(),
                Action = entry.State.ToString(),
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                OldValues = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                    ? JsonSerializer.Serialize(entry.OriginalValues.ToObject())
                    : null,
                NewValues = entry.State == EntityState.Added || entry.State == EntityState.Modified
                    ? JsonSerializer.Serialize(entry.CurrentValues.ToObject())
                    : null
            };

            PendingLogs.Add(log);
        }

        return new(result);
    }
}
