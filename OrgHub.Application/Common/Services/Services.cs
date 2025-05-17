using OrgHub.Application.Common.Interfaces;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Common.Services
{
    public class Service<T, TDto> : IService<T, TDto>
            where T : class
            where TDto : class
    {
        private readonly IRepository<T> _repository;
        private readonly Func<TDto, T> _mapToEntity;
        private readonly Func<T, TDto> _mapToDto;

        public Service(IRepository<T> repository, Func<TDto, T> mapToEntity, Func<T, TDto> mapToDto)
        {
            _repository = repository;
            _mapToEntity = mapToEntity;
            _mapToDto = mapToDto;
        }

        public async Task<List<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(_mapToDto).ToList();
        }

        public async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapToDto(entity);
        }

        public async Task<TDto> AddAsync(TDto dto)
        {
            var entity = _mapToEntity(dto);
            await _repository.AddAsync(entity);
            return _mapToDto(entity);
        }

        public async Task UpdateAsync(TDto dto)
        {
            var entity = _mapToEntity(dto);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
