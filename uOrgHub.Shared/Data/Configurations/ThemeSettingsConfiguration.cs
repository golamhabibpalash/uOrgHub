using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Data.Configurations;

public class ThemeSettingsConfiguration : IEntityTypeConfiguration<ThemeSettings>
{
    public void Configure(EntityTypeBuilder<ThemeSettings> builder)
    {
        builder.ToTable("sys_theme_settings");
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
