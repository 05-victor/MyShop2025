using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared;
using MyShop.Shared.DTOs.Commons;

namespace MyShop.Data.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ShopContext _context;

    public UserRepository(ShopContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Profile)
            .ToListAsync();
    }

    public async Task<PagedResult<User>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Users
            .Include(u => u.Profile)
            .Include(u => u.Roles);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Load navigation properties after save
        await _context.Entry(user)
            .Collection(u => u.Roles)
            .LoadAsync();
        
        await _context.Entry(user)
            .Reference(u => u.Profile)
            .LoadAsync();
        
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string username, string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username || u.Email == email);
    }
}
