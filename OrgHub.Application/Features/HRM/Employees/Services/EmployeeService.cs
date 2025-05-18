using AutoMapper;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Application.Features.Employees.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Employees.Services
{
    public class EmployeeService : Service<Employee, EmployeeDto>, IEmployeeService
    {
        private readonly IRepository<Employee> _repository;
        private readonly IMapper _mapper;
        public EmployeeService(IRepository<Employee> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<EmployeeDto>> GetByInfoAsync(string info)
        {
            // Fix: Ensure ToListAsync is used correctly with IQueryable by adding the necessary using directive
            var filteredEmployees = _repository.Table
                .Where(t => t.Name.ToLower().Contains(info.ToLower())
                || t.EmployeeCode.ToLower().Contains(info.ToLower())
                || t.Designation.ToLower().Contains(info.ToLower()))
                .ToList();

            return _mapper.Map<List<EmployeeDto>>(filteredEmployees);
        }
    }
}
