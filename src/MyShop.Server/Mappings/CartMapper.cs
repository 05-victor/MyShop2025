using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Mappings;

/// <summary>
/// Mapper for converting CartItem entities to response DTOs
/// </summary>
public static class CartMapper
{
    /// <summary>
    /// Maps a CartItem entity to CartItemResponse DTO
    /// </summary>
    public static CartItemResponse ToCartItemResponse(CartItem cartItem)
    {
        return new CartItemResponse
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            ProductName = cartItem.Product?.Name ?? string.Empty,
            ProductImage = cartItem.Product?.ImageUrl,
            Price = cartItem.Product?.SellingPrice ?? 0,
            Quantity = cartItem.Quantity,
            Subtotal = (cartItem.Product?.SellingPrice ?? 0) * cartItem.Quantity,
            StockAvailable = cartItem.Product?.Quantity ?? 0,
            CategoryName = cartItem.Product?.Category?.Name,
            AddedAt = cartItem.CreatedAt
        };
    }

    /// <summary>
    /// Maps a collection of CartItem entities to CartResponse DTO
    /// </summary>
    public static CartResponse ToCartResponse(IEnumerable<CartItem> cartItems, Guid userId)
    {
        var itemResponses = cartItems.Select(ToCartItemResponse).ToList();
        var subtotal = itemResponses.Sum(i => i.Subtotal);
        var tax = subtotal * 0.1m; // 10% tax
        var shippingFee = subtotal > 500000 ? 0 : 30000; // Free shipping over 500k VND
        var total = subtotal + tax + shippingFee;

        return new CartResponse
        {
            UserId = userId,
            Items = itemResponses,
            Subtotal = subtotal,
            Tax = tax,
            ShippingFee = shippingFee,
            Total = total,
            ItemCount = itemResponses.Sum(i => i.Quantity)
        };
    }
}
