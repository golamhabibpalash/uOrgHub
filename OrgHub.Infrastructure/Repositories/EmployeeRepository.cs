using Microsoft.EntityFrameworkCore;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities.HRM;
using OrgHub.Infrastructure.Persistence;

namespace OrgHub.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HRM_Employee>> GetAllAsync() =>
            await _context.HRM_Employees.ToListAsync();

        public async Task<List<HRM_Employee>> GetAllByInfoAsync(string info) =>
            await _context.HRM_Employees
            .Where(s => s.Name.ToLower().Contains(info.ToLower())
            || s.Designation.Title.ToLower().Contains(info.ToLower())
            || s.EmployeeCode.ToLower().Contains(info.ToLower()))
            .ToListAsync();

        public async Task<HRM_Employee?> GetByIdAsync(int id) =>
            await _context.HRM_Employees.FindAsync(id);

        public async Task<HRM_Employee> AddAsync(HRM_Employee employee)
        {
            await _context.HRM_Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateAsync(HRM_Employee employee)
        {
            _context.HRM_Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await _context.HRM_Employees.FindAsync(id);
            if (emp != null)
            {
                _context.HRM_Employees.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }
    }
}
