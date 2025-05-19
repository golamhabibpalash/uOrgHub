using AutoMapper;
using OrgHub.Application.Common.Interfaces;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Common.Services
{
    public class Service<T, TDto> : IService<T, TDto>
        where T : class
        where TDto : class
    {
        private readonly IRepository<T> _repository;
        private readonly IMapper _mapper;

        public Service(IRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<TDto>>(entities);
        }

        public async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TDto>(entity);
        }

        public async Task<TDto> AddAsync(TDto dto)
        {
            var entity = _mapper.Map<T>(dto);
            var savedEntity = await _repository.AddAsync(entity);
            return _mapper.Map<TDto>(savedEntity);
        }

        public async Task UpdateAsync(TDto dto)
        {
            var entity = _mapper.Map<T>(dto);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(entity);
        }
    }
}
