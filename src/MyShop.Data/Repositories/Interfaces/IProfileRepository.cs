using MyShop.Data.Entities;

namespace MyShop.Data.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<Profile?> GetByUserIdAsync(Guid userId);
        Task<Profile> CreateAsync(Profile profile);
        Task<Profile> UpdateAsync(Profile profile);
        Task DeleteAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId);
    }
}