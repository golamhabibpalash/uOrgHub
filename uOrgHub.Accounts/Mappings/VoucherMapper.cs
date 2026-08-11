using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

[Mapper]
public partial class VoucherMapper
{
    [MapperIgnoreTarget(nameof(Voucher.CostCenter))]
    public partial Voucher ToEntity(CreateVoucherDto dto);

    [MapperIgnoreTarget(nameof(Voucher.Id))]
    [MapperIgnoreTarget(nameof(Voucher.VoucherNumber))]
    [MapperIgnoreTarget(nameof(Voucher.VoucherType))]
    [MapperIgnoreTarget(nameof(Voucher.Status))]
    [MapperIgnoreTarget(nameof(Voucher.CostCenter))]
    [MapperIgnoreTarget(nameof(Voucher.JournalEntry))]
    [MapperIgnoreTarget(nameof(Voucher.JournalEntryId))]
    [MapperIgnoreTarget(nameof(Voucher.SubmittedBy))]
    [MapperIgnoreTarget(nameof(Voucher.SubmittedAt))]
    [MapperIgnoreTarget(nameof(Voucher.ApprovedBy))]
    [MapperIgnoreTarget(nameof(Voucher.ApprovedAt))]
    [MapperIgnoreTarget(nameof(Voucher.RejectedBy))]
    [MapperIgnoreTarget(nameof(Voucher.RejectedAt))]
    [MapperIgnoreTarget(nameof(Voucher.RejectReason))]
    [MapperIgnoreTarget(nameof(Voucher.PostedBy))]
    [MapperIgnoreTarget(nameof(Voucher.PostedAt))]
    [MapperIgnoreTarget(nameof(Voucher.CreatedAt))]
    [MapperIgnoreTarget(nameof(Voucher.CreatedBy))]
    [MapperIgnoreTarget(nameof(Voucher.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Voucher.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Voucher.IsDeleted))]
    [MapperIgnoreTarget(nameof(Voucher.DeletedAt))]
    [MapperIgnoreTarget(nameof(Voucher.DeletedBy))]
    public partial void UpdateEntity(UpdateVoucherDto dto, Voucher entity);

    [MapProperty([nameof(Voucher.FiscalYear), nameof(FiscalYear.Name)], [nameof(VoucherResponseDto.FiscalYearName)])]
    [MapProperty([nameof(Voucher.CostCenter), nameof(CostCenter.Name)], [nameof(VoucherResponseDto.CostCenterName)])]
    [MapProperty([nameof(Voucher.CostCenter), nameof(CostCenter.Code)], [nameof(VoucherResponseDto.CostCenterCode)])]
    [MapProperty([nameof(Voucher.DebitAccount), nameof(ChartOfAccount.AccountName)], [nameof(VoucherResponseDto.DebitAccountName)])]
    [MapProperty([nameof(Voucher.CreditAccount), nameof(ChartOfAccount.AccountName)], [nameof(VoucherResponseDto.CreditAccountName)])]
    [MapProperty([nameof(Voucher.JournalEntry), nameof(JournalEntry.EntryNumber)], [nameof(VoucherResponseDto.JournalEntryNumber)])]
    public partial VoucherResponseDto ToDto(Voucher entity);

    /// <summary>
    /// Bank details are stamped on afterwards by the query handler — they come from the
    /// <see cref="BankAccount"/> attached to the account, which is not reachable from here.
    /// </summary>
    [MapProperty([nameof(ChartOfAccount.AccountGroup), nameof(AccountGroup.Name)], [nameof(VoucherAccountOptionDto.AccountGroupName)])]
    [MapperIgnoreTarget(nameof(VoucherAccountOptionDto.IsBankLinked))]
    [MapperIgnoreTarget(nameof(VoucherAccountOptionDto.BankName))]
    [MapperIgnoreTarget(nameof(VoucherAccountOptionDto.AccountNumber))]
    [MapperIgnoreTarget(nameof(VoucherAccountOptionDto.GroupLabel))]
    public partial VoucherAccountOptionDto ToAccountOptionDto(ChartOfAccount entity);
}
