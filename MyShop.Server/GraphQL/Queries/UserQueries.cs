using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Shared;

namespace MyShop.Server.GraphQL.Queries;

public class UserQueries
{
    /// <summary>
    /// Get all users
    /// </summary>
    public async Task<List<User>> GetUsers([Service] ShopContext context)
    {
        return await context.Users.ToListAsync();
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserById([Service] ShopContext context, int id)
    {
        return await context.Users.FindAsync(id);
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<User?> GetUserByUsername([Service] ShopContext context, string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetUserByEmail([Service] ShopContext context, string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
