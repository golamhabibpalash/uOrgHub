using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

// Only unmapped *targets* are reported: the entities carry audit/company columns no DTO exposes.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class FixedAssetMapper
{
    // ── Asset categories ──
    [MapProperty("AssetAccount.AccountName", nameof(AssetCategoryResponseDto.AssetAccountName))]
    [MapProperty("AccumulatedDepreciationAccount.AccountName", nameof(AssetCategoryResponseDto.AccumulatedDepreciationAccountName))]
    [MapProperty("DepreciationExpenseAccount.AccountName", nameof(AssetCategoryResponseDto.DepreciationExpenseAccountName))]
    [MapProperty("HireExpenseAccount.AccountName", nameof(AssetCategoryResponseDto.HireExpenseAccountName))]
    [MapProperty("HireRecoveryAccount.AccountName", nameof(AssetCategoryResponseDto.HireRecoveryAccountName))]
    public partial AssetCategoryResponseDto ToDto(AssetCategory entity);

    [MapperIgnoreTarget(nameof(AssetCategory.Code))]
    public partial AssetCategory ToEntity(CreateAssetCategoryDto dto);

    public partial void UpdateEntity(UpdateAssetCategoryDto dto, AssetCategory entity);

    // ── Fixed assets ──
    // CategoryName, VendorName, BillBillNumber and CostCenterName flatten from the navigations.
    [MapperIgnoreTarget(nameof(FixedAssetResponseDto.HasPostedDepreciation))]
    public partial FixedAssetResponseDto ToDto(FixedAsset entity);

    // Code, defaults and the opening-depreciation position are resolved by the create handler.
    [MapperIgnoreTarget(nameof(FixedAsset.AssetCode))]
    [MapperIgnoreTarget(nameof(FixedAsset.DepreciationStartDate))]
    [MapperIgnoreTarget(nameof(FixedAsset.SalvageValue))]
    [MapperIgnoreTarget(nameof(FixedAsset.UsefulLifeMonths))]
    [MapperIgnoreTarget(nameof(FixedAsset.DepreciationMethod))]
    [MapperIgnoreTarget(nameof(FixedAsset.AccumulatedDepreciation))]
    [MapperIgnoreTarget(nameof(FixedAsset.LastDepreciationDate))]
    [MapperIgnoreSource(nameof(CreateFixedAssetDto.OpeningDepreciatedUpTo))]
    public partial FixedAsset ToEntity(CreateFixedAssetDto dto);

    public partial void UpdateEntity(UpdateFixedAssetDto dto, FixedAsset entity);

    // ── Depreciation runs ──
    // JournalEntryEntryNumber flattens from JournalEntry.EntryNumber.
    [MapProperty(nameof(DepreciationRun.UpdatedBy), nameof(DepreciationRunResponseDto.ReversedBy))]
    public partial DepreciationRunResponseDto ToDto(DepreciationRun entity);

    [MapProperty("FixedAsset.AssetCode", nameof(DepreciationLineDto.AssetCode))]
    [MapProperty("FixedAsset.Name", nameof(DepreciationLineDto.AssetName))]
    [MapProperty("FixedAsset.Category.Name", nameof(DepreciationLineDto.CategoryName))]
    [MapProperty("FixedAsset.PurchaseCost", nameof(DepreciationLineDto.PurchaseCost))]
    public partial DepreciationLineDto ToDto(DepreciationRunLine line);

    [MapProperty("DepreciationRun.RunNumber", nameof(FixedAssetDepreciationHistoryDto.RunNumber))]
    [MapProperty("DepreciationRun.PeriodEndDate", nameof(FixedAssetDepreciationHistoryDto.PeriodEndDate))]
    [MapProperty("DepreciationRun.Status", nameof(FixedAssetDepreciationHistoryDto.Status))]
    public partial FixedAssetDepreciationHistoryDto ToHistoryDto(DepreciationRunLine line);

    // ── Deployments ──
    // FixedAssetAssetCode, FixedAssetName and CostCenterName flatten from the navigations; the
    // charged-to position comes from posted hire runs and is filled in by the query handler.
    [MapperIgnoreTarget(nameof(AssetDeploymentResponseDto.ChargedUpTo))]
    [MapperIgnoreTarget(nameof(AssetDeploymentResponseDto.TotalHireCharged))]
    public partial AssetDeploymentResponseDto ToDto(AssetDeployment entity);

    // ── Hire charge runs ──
    [MapProperty(nameof(HireChargeRun.UpdatedBy), nameof(HireChargeRunResponseDto.ReversedBy))]
    [MapperIgnoreTarget(nameof(HireChargeRunResponseDto.Warning))]
    public partial HireChargeRunResponseDto ToDto(HireChargeRun entity);

    [MapProperty("AssetDeployment.FixedAsset.AssetCode", nameof(HireChargeLineDto.AssetCode))]
    [MapProperty("AssetDeployment.FixedAsset.Name", nameof(HireChargeLineDto.AssetName))]
    [MapProperty("AssetDeployment.CostCenter.Name", nameof(HireChargeLineDto.ProjectName))]
    public partial HireChargeLineDto ToDto(HireChargeRunLine line);
}
