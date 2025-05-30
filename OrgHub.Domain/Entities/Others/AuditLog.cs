using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Domain.Entities.Others;

public class AuditLog
{
    public int Id { get; set; }

    public string TableName { get; set; } = null!;

    public string KeyValues { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string Action { get; set; } = null!;

    public string? UserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
