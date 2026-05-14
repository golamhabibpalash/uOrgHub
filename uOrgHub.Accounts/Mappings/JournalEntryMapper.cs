using Riok.Mapperly.Abstractions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Mappings;

[Mapper]
public partial class JournalEntryMapper
{
    public partial JournalEntryResponseDto ToDto(JournalEntry entity);
    public partial JournalEntry ToEntity(CreateJournalEntryDto dto);
    public partial void UpdateEntity(UpdateJournalEntryDto dto, JournalEntry entity);
}