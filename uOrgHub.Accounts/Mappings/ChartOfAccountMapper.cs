using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

[Mapper]
public partial class ChartOfAccountMapper
{
    public ChartOfAccountResponseDto ToDto(ChartOfAccount entity)
    {
        var dto = MapToDto(entity);
        dto.AccountGroupName = entity.AccountGroup?.Name;
        dto.ParentAccountName = entity.ParentAccount?.AccountName;
        return dto;
    }

    private partial ChartOfAccountResponseDto MapToDto(ChartOfAccount entity);

    public partial ChartOfAccount ToEntity(CreateChartOfAccountDto dto);
    public partial void UpdateEntity(UpdateChartOfAccountDto dto, ChartOfAccount entity);
    public partial JournalEntryLineResponseDto ToLedgerLineDto(JournalEntryLine entity);
}