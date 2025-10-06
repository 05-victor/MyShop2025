using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Shared;
using MyShop.Shared.DTOs.User;

namespace MyShop.Server.GraphQL.Mutations;

public class UserMutations
{
    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task<User> CreateUser([Service] ShopContext context, CreateUserInput input)
    {
        var user = new User
        {
            Username = input.Username,
            Password = input.Password, // In production, hash the password!
            FullName = input.FullName,
            Email = input.Email,
            Photo = input.Photo,
            Role = input.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.MinValue
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    public async Task<User?> UpdateUser([Service] ShopContext context, UpdateUserInput input)
    {
        var user = await context.Users.FindAsync(input.Id);
        
        if (user == null)
        {
            return null;
        }

        // Update only provided fields
        if (input.Username != null)
            user.Username = input.Username;
        
        if (input.Password != null)
            user.Password = input.Password; // In production, hash the password!
        
        if (input.FullName != null)
            user.FullName = input.FullName;
        
        if (input.Email != null)
            user.Email = input.Email;
        
        if (input.Photo != null)
            user.Photo = input.Photo;
        
        if (input.Role != null)
            user.Role = input.Role;

        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Delete a user (hard delete)
    /// </summary>
    public async Task<bool> DeleteUser([Service] ShopContext context, int id)
    {
        var user = await context.Users.FindAsync(id);
        
        if (user == null)
        {
            return false;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Soft delete a user (set DeletedAt timestamp)
    /// </summary>
    public async Task<User?> SoftDeleteUser([Service] ShopContext context, int id)
    {
        var user = await context.Users.FindAsync(id);
        
        if (user == null)
        {
            return null;
        }

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return user;
    }
}
