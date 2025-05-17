using Microsoft.EntityFrameworkCore;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;
using OrgHub.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Infrastructure.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly AppDbContext _context;
        public EquipmentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Equipment> AddAsync(Equipment equipment)
        {
            await _context.AddAsync(equipment);
            await _context.SaveChangesAsync();
            return equipment;
        }

        public async Task DeleteAsync(int id)
        {
            var existingEquipment = await _context.Equipments.FindAsync(id);
            if (existingEquipment!=null)
            {
                _context.Equipments.Remove(existingEquipment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Equipment>> GetAllAsync()
        {
            var allEquipments = await _context.Equipments.ToListAsync();
            return allEquipments;
        }

        public async Task<Equipment?> GetByIdAsync(int id)
        {
            var existingEquipment = await _context.Equipments.FindAsync(id);
            return existingEquipment;
        }

        public async Task UpdateAsync(Equipment equipment)
        {
            _context.Equipments.Update(equipment);
            await _context.SaveChangesAsync();
        }
    }
}
