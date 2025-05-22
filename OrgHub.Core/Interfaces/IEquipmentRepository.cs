using OrgHub.Domain.Entities.FixedAssets;


namespace OrgHub.Core.Interfaces;
public interface IEquipmentRepository
{
    Task<List<FixedAssets_Equipment>> GetAllAsync();
    Task<FixedAssets_Equipment?> GetByIdAsync(int id);
    Task<FixedAssets_Equipment> AddAsync(FixedAssets_Equipment equipment);
    Task UpdateAsync(FixedAssets_Equipment equipment);
    Task DeleteAsync(int id);
}