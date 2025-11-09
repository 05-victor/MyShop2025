using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;

namespace MyShop.Data.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly ShopContext _context;

    public RoleRepository(ShopContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.RoleAuthorities)
                .ThenInclude(ra => ra.Authority)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.RoleAuthorities)
                .ThenInclude(ra => ra.Authority)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.Roles
            .AnyAsync(r => r.Name == name);
    }
}
