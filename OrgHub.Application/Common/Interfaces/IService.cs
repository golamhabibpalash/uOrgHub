namespace OrgHub.Application.Common.Interfaces;

public interface IService<T, TDto>
{
    Task<List<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(int id);
    Task<TDto> AddAsync(TDto dto);
    Task UpdateAsync(TDto dto);
    Task DeleteAsync(int id);
}
