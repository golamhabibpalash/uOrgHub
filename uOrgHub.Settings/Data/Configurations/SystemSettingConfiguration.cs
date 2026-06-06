using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Settings.Models.Entities;

namespace uOrgHub.Settings.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("sett_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.DataType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.Category);
    }
}
