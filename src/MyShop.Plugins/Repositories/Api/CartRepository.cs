using MyShop.Plugins.Adapters;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Cart;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;
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

    public async Task<Result<IEnumerable<CartItem>>> GetCartItemsAsync(Guid userId)
    {
        try
        {
            var response = await _api.GetMyCartAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result?.Items != null)
                {
                    var items = CartAdapter.ToModelList(apiResponse.Result);
                    return Result<IEnumerable<CartItem>>.Success(items);
                }
            }

            return Result<IEnumerable<CartItem>>.Failure("Failed to retrieve cart items");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CartItem>>.Failure($"Error retrieving cart items: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddToCartAsync(Guid userId, Guid productId, int quantity = 1)
    {
        try
        {
            var request = new AddToCartRequest
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _api.AddItemAsync(request);
            if (response.IsSuccessStatusCode && response.Content?.Result != null)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to add item to cart");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error adding item to cart: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateQuantityAsync(Guid userId, Guid productId, int quantity)
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
            if (response.IsSuccessStatusCode && response.Content?.Result != null)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to update cart item quantity");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating cart item quantity: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        try
        {
            // Note: API expects itemId, not productId
            var response = await _api.RemoveItemAsync(productId);
            if (response.IsSuccessStatusCode && response.Content?.Result == true)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to remove item from cart");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing item from cart: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ClearCartAsync(Guid userId)
    {
        try
        {
            var response = await _api.ClearCartAsync();
            if (response.IsSuccessStatusCode && response.Content?.Result == true)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to clear cart");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error clearing cart: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetCartCountAsync(Guid userId)
    {
        try
        {
            var response = await _api.GetMyCartAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return Result<int>.Success(apiResponse.Result.ItemCount);
                }
            }

            return Result<int>.Failure("Failed to retrieve cart count");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error retrieving cart count: {ex.Message}");
        }
    }

    public async Task<Result<CartSummary>> GetCartSummaryAsync(Guid userId)
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
                    var summary = new CartSummary
                    {
                        ItemCount = data.ItemCount,
                        Subtotal = data.Subtotal,
                        Tax = data.Tax,
                        ShippingFee = data.ShippingFee,
                        Total = data.Total
                    };
                    return Result<CartSummary>.Success(summary);
                }
            }

            return Result<CartSummary>.Failure("Failed to retrieve cart summary");
        }
        catch (Exception ex)
        {
            return Result<CartSummary>.Failure($"Error retrieving cart summary: {ex.Message}");
        }
    }
}
