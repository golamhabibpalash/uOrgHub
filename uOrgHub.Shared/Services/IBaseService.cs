using uOrgHub.Shared.Models;

namespace uOrgHub.Shared.Services;

public interface IBaseService<TDto, TCreateDto, TUpdateDto>
{
    Task<PagedResult<TDto>> GetAllAsync(PaginationRequest request);
    Task<TDto> GetByIdAsync(Guid id);
    Task<TDto> CreateAsync(TCreateDto dto);
    Task<TDto> UpdateAsync(Guid id, TUpdateDto dto);
    Task DeleteAsync(Guid id);
}