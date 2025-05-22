using Microsoft.EntityFrameworkCore;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities.FixedAssets;
using OrgHub.Infrastructure.Persistence;

namespace OrgHub.Infrastructure.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly AppDbContext _context;
        public EquipmentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<FixedAssets_Equipment> AddAsync(FixedAssets_Equipment equipment)
        {
            await _context.AddAsync(equipment);
            await _context.SaveChangesAsync();
            return equipment;
        }

        public async Task DeleteAsync(int id)
        {
            var existingEquipment = await _context.FixedAssets_Equipments.FindAsync(id);
            if (existingEquipment!=null)
            {
                _context.FixedAssets_Equipments.Remove(existingEquipment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<FixedAssets_Equipment>> GetAllAsync()
        {
            var allEquipments = await _context.FixedAssets_Equipments.ToListAsync();
            return allEquipments;
        }

        public async Task<FixedAssets_Equipment?> GetByIdAsync(int id)
        {
            var existingEquipment = await _context.FixedAssets_Equipments.FindAsync(id);
            return existingEquipment;
        }

        public async Task UpdateAsync(FixedAssets_Equipment equipment)
        {
            _context.FixedAssets_Equipments.Update(equipment);
            await _context.SaveChangesAsync();
        }
    }
}
