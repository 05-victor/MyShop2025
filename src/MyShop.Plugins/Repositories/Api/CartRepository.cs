using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Cart;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Cart Repository implementation
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly ICartApi _api;

    public CartRepository(ICartApi api)
    {
        _api = api;
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid userId)
    {
        try
        {
            var response = await _api.GetMyCartAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result?.Items != null)
                {
                    return apiResponse.Result.Items.Select(MapToCartItem);
                }
            }

            return Enumerable.Empty<CartItem>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<CartItem>();
        }
    }

    public async Task<bool> AddToCartAsync(Guid userId, Guid productId, int quantity = 1)
    {
        try
        {
            var request = new AddToCartRequest
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _api.AddItemAsync(request);
            return response.IsSuccessStatusCode && response.Content?.Result != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateQuantityAsync(Guid userId, Guid productId, int quantity)
    {
        try
        {
            // Note: API expects itemId, not productId. 
            // This is a simplified implementation - may need adjustment based on actual backend API
            var request = new UpdateCartItemRequest
            {
                Quantity = quantity
            };

            var response = await _api.UpdateItemAsync(productId, request);
            return response.IsSuccessStatusCode && response.Content?.Result != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        try
        {
            // Note: API expects itemId, not productId
            var response = await _api.RemoveItemAsync(productId);
            return response.IsSuccessStatusCode && response.Content?.Result == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ClearCartAsync(Guid userId)
    {
        try
        {
            var response = await _api.ClearCartAsync();
            return response.IsSuccessStatusCode && response.Content?.Result == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<int> GetCartCountAsync(Guid userId)
    {
        try
        {
            var response = await _api.GetMyCartAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result.ItemCount;
                }
            }

            return 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public async Task<CartSummary> GetCartSummaryAsync(Guid userId)
    {
        try
        {
            var response = await _api.GetMyCartAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var data = apiResponse.Result;
                    return new CartSummary
                    {
                        ItemCount = data.ItemCount,
                        Subtotal = data.Subtotal,
                        Tax = data.Tax,
                        ShippingFee = data.ShippingFee,
                        Total = data.Total
                    };
                }
            }

            return new CartSummary();
        }
        catch (Exception)
        {
            return new CartSummary();
        }
    }

    /// <summary>
    /// Map CartItemResponse DTO to CartItem domain model
    /// </summary>
    private static CartItem MapToCartItem(MyShop.Shared.DTOs.Responses.CartItemResponse dto)
    {
        return new CartItem
        {
            Id = dto.Id,
            UserId = Guid.Empty, // Not provided by API response
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            ProductImage = dto.ProductImage,
            Price = dto.Price,
            Quantity = dto.Quantity,
            CategoryName = dto.CategoryName,
            StockAvailable = dto.StockAvailable,
            AddedAt = DateTime.UtcNow
        };
    }
}
