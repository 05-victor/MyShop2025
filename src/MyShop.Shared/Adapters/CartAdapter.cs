using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Shared.Adapters;

/// <summary>
/// Adapter for mapping Cart DTOs to domain models
/// </summary>
public static class CartAdapter
{
    /// <summary>
    /// Maps CartItemResponse DTO to CartItem model
    /// </summary>
    public static CartItem ToModel(CartItemResponse dto)
    {
        return new CartItem
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            ProductImage = dto.ProductImage,
            Price = dto.Price,
            Quantity = dto.Quantity,
            CategoryName = dto.CategoryName,
            StockAvailable = dto.StockAvailable,
            // Note: UserId, AddedAt, CreatedAt, UpdatedAt are not in CartItemResponse
            // These will be set by the repository or business logic if needed
        };
    }

    /// <summary>
    /// Maps CartResponse DTO to list of CartItem models
    /// </summary>
    public static List<CartItem> ToModelList(CartResponse dto)
    {
        var list = dto.Items.Select(ToModel).ToList();

        // Assign UserId and timestamps from CartResponse to each item if available
        foreach (var item in list)
        {
            item.UserId = dto.UserId;
            // try to preserve AddedAt if provided in DTO item
            var dtoItem = dto.Items.FirstOrDefault(i => i.Id == item.Id);
            if (dtoItem != null && dtoItem.AddedAt.HasValue)
            {
                item.AddedAt = dtoItem.AddedAt.Value;
                item.CreatedAt = dtoItem.AddedAt.Value;
            }
            else
            {
                item.AddedAt = DateTime.UtcNow;
                item.CreatedAt = DateTime.UtcNow;
            }
        }

        return list;
    }

    /// <summary>
    /// Maps CartResponse DTO to CartSummary model
    /// </summary>
    public static CartSummary ToSummary(CartResponse dto)
    {
        return new CartSummary
        {
            ItemCount = dto.ItemCount,
            Subtotal = dto.Subtotal,
            Tax = dto.Tax,
            ShippingFee = dto.ShippingFee,
            Total = dto.Total
        };
    }
}
