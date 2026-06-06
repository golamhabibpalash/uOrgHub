using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Settings.Models.Entities;

namespace uOrgHub.Settings.Data.Configurations;

public class ValidationRuleConfiguration : IEntityTypeConfiguration<ValidationRule>
{
    public void Configure(EntityTypeBuilder<ValidationRule> builder)
    {
        builder.ToTable("sett_validation_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RuleType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RuleValue).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.Severity).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.HasIndex(x => new { x.EntityType, x.FieldName });
    }
}
