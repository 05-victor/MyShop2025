using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;

namespace MyShop.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for cart operations
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly ShopContext _context;

    public CartRepository(ShopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.CreatedAt)
            .ToListAsync();
    }

    public async Task<CartItem?> GetCartItemAsync(Guid userId, Guid productId)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
    }

    public async Task<CartItem> AddToCartAsync(Guid userId, Guid productId, int quantity)
    {
        // Check if item already exists
        var existingItem = await GetCartItemAsync(userId, productId);

        if (existingItem != null)
        {
            // Update quantity if item exists
            existingItem.Quantity += quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingItem;
        }

        // Create new cart item
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        // Load navigation properties
        await _context.Entry(cartItem)
            .Reference(ci => ci.Product)
            .LoadAsync();

        if (cartItem.Product != null)
        {
            await _context.Entry(cartItem.Product)
                .Reference(p => p.Category)
                .LoadAsync();
        }

        return cartItem;
    }

    public async Task<CartItem> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity)
    {
        var cartItem = await GetCartItemAsync(userId, productId);

        if (cartItem == null)
        {
            throw new InvalidOperationException("Cart item not found");
        }

        cartItem.Quantity = quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return cartItem;
    }

    public async Task<bool> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (cartItem == null)
        {
            return false;
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ClearCartAsync(Guid userId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return false;
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return true;
    }
}
