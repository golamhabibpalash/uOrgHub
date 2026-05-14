using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

[Mapper]
public partial class FiscalYearMapper
{
    public partial FiscalYearResponseDto ToDto(FiscalYear entity);
    public partial FiscalYear ToEntity(CreateFiscalYearDto dto);
    public partial void UpdateEntity(UpdateFiscalYearDto dto, FiscalYear entity);
}