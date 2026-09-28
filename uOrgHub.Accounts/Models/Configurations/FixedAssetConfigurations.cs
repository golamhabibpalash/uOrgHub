using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> b)
    {
        b.HasKey(x => x.Id);
        // Composite for the same reason as BillConfiguration: each sister concern numbers its own.
        b.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.AssetAccount).WithMany()
         .HasForeignKey(x => x.AssetAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AccumulatedDepreciationAccount).WithMany()
         .HasForeignKey(x => x.AccumulatedDepreciationAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DepreciationExpenseAccount).WithMany()
         .HasForeignKey(x => x.DepreciationExpenseAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FixedAssetConfiguration : IEntityTypeConfiguration<FixedAsset>
{
    public void Configure(EntityTypeBuilder<FixedAsset> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CompanyId, x.AssetCode }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Category).WithMany(x => x.Assets)
         .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        // No inverse Vendor.FixedAssets: Vendor lives in uOrgHub.Shared (see BillConfiguration).
        b.HasOne(x => x.Vendor).WithMany()
         .HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Bill).WithMany()
         .HasForeignKey(x => x.BillId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.CostCenter).WithMany()
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);

        b.Ignore(x => x.BookValue);
    }
}

public class DepreciationRunConfiguration : IEntityTypeConfiguration<DepreciationRun>
{
    public void Configure(EntityTypeBuilder<DepreciationRun> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CompanyId, x.RunNumber }).IsUnique();
        // Not unique: a reversed run leaves its period free to be run again.
        b.HasIndex(x => new { x.CompanyId, x.PeriodYear, x.PeriodMonth });
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.JournalEntry).WithMany()
         .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(x => x.Lines).WithOne(x => x.DepreciationRun)
         .HasForeignKey(x => x.DepreciationRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DepreciationRunLineConfiguration : IEntityTypeConfiguration<DepreciationRunLine>
{
    public void Configure(EntityTypeBuilder<DepreciationRunLine> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.FixedAsset).WithMany()
         .HasForeignKey(x => x.FixedAssetId).OnDelete(DeleteBehavior.Restrict);
    }
}
