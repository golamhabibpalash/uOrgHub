using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Features.HRM.Employees.Services
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
            var filteredEmployees = await _repository.Table()
                .Where(t => t.Name.ToLower().Contains(info.ToLower())
                || t.EmployeeCode.ToLower().Contains(info.ToLower())
                || t.Designation.Title.ToLower().Contains(info.ToLower()))
                .ToListAsync();

            return _mapper.Map<List<EmployeeDto>>(filteredEmployees);
        }
    }
}
