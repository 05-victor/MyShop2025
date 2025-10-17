using MyShop.Data.Entities;

namespace MyShop.Data.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<bool> ExistsAsync(string name);
}
