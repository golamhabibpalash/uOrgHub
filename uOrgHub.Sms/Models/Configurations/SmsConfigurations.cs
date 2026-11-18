using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Sms.Models.Entities;

namespace uOrgHub.Sms.Models.Configurations;

public class SmsGatewayConfiguration : IEntityTypeConfiguration<SmsGateway>
{
    public void Configure(EntityTypeBuilder<SmsGateway> b)
    {
        b.HasKey(x => x.Id);
        // One active gateway per company. Filtered so soft-deleted and non-default rows don't count.
        // (Plain CompanyId lookups use the CompanyId + Name index below.)
        b.HasIndex(x => x.CompanyId, "IX_sms_gateways_CompanyId_Default").IsUnique()
            .HasFilter("\"IsDefault\" = TRUE AND \"IsDeleted\" = FALSE");
        b.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
    }
}

public class SmsSendRequestConfiguration : IEntityTypeConfiguration<SmsSendRequest>
{
    public void Configure(EntityTypeBuilder<SmsSendRequest> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CompanyId, x.IdempotencyKey }).IsUnique();
        b.HasIndex(x => new { x.CompanyId, x.CreatedAt });
        b.HasOne(x => x.Gateway).WithMany().HasForeignKey(x => x.GatewayId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SmsMessageConfiguration : IEntityTypeConfiguration<SmsMessage>
{
    public void Configure(EntityTypeBuilder<SmsMessage> b)
    {
        b.HasKey(x => x.Id);
        // History list and analytics: newest first within a company, optionally by status/gateway.
        b.HasIndex(x => new { x.CompanyId, x.CreatedAt });
        b.HasIndex(x => new { x.CompanyId, x.Status, x.CreatedAt });
        b.HasIndex(x => new { x.CompanyId, x.GatewayId, x.CreatedAt });
        // Dispatcher: queued work due now; delivery-report polling of submitted messages.
        b.HasIndex(x => new { x.Status, x.NextAttemptAt });
        b.HasIndex(x => new { x.Status, x.SubmittedAt });
        b.HasIndex(x => x.ProviderMessageId);
        b.HasIndex(x => x.Recipient);
        b.HasOne(x => x.SendRequest).WithMany(r => r.Messages).HasForeignKey(x => x.SendRequestId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Gateway).WithMany().HasForeignKey(x => x.GatewayId).OnDelete(DeleteBehavior.Restrict);
    }
}
