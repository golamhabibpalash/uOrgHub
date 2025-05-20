using OrgHub.Domain.Entities.FixedAssets;


namespace OrgHub.Core.Interfaces;
public interface IEquipmentRepository
{
    Task<List<Equipment>> GetAllAsync();
    Task<Equipment?> GetByIdAsync(int id);
    Task<Equipment> AddAsync(Equipment equipment);
    Task UpdateAsync(Equipment equipment);
    Task DeleteAsync(int id);
}