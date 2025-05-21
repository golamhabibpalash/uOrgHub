using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace OrgHub.Infrastructure.Persistence.Others;

public class AuditLoggingInterceptor : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var entityName = entry.Entity.GetType().Name;
                    var entityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;
                    var changes = entry.State == EntityState.Modified
                        ? string.Join(", ", entry.Properties.Where(p => p.IsModified).Select(p => $"{p.Metadata.Name}: {p.OriginalValue} -> {p.CurrentValue}"))
                        : null;

                    Log.Information("Audit: {Entity}, EntityId: {EntityId}, Action: {Action}, Changes: {Changes}",
                        entityName, entityId, entry.State.ToString(), changes);
                }
            }
        }

        return base.SavedChanges(eventData, result);
    }
}
