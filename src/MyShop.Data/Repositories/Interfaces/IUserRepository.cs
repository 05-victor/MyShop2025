using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Commons;

namespace MyShop.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<PagedResult<User>> GetAllAsync(int pageNumber, int pageSize);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string username, string email);
    Task<bool> HasAdminAsync();
}
