using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Models;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Services;

public interface IJournalEntryService : IBaseService<JournalEntryResponseDto, CreateJournalEntryDto, UpdateJournalEntryDto>
{
    Task<JournalEntryResponseDto> PostAsync(Guid id, string postedBy);
    Task<JournalEntryResponseDto> CancelAsync(Guid id);
    Task<PagedResult<JournalEntryResponseDto>> GetAllAsync(PaginationRequest request, DateTime? dateFrom, DateTime? dateTo);
}