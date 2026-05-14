using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Services;

public interface IJournalEntryService : IBaseService<JournalEntryResponseDto, CreateJournalEntryDto, UpdateJournalEntryDto>
{
    Task<JournalEntryResponseDto> PostAsync(Guid id, string postedBy);
    Task<JournalEntryResponseDto> CancelAsync(Guid id);
}