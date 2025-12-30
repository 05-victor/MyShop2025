using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;
using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for shopping cart - delegates to MockCartData
/// </summary>
public class MockCartRepository : ICartRepository
{
    private readonly IProductRepository _productRepository;

    public MockCartRepository(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IEnumerable<CartItem>>> GetCartItemsAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Got {items.Count} items for user {userId}");
            return Result<IEnumerable<CartItem>>.Success(items);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartItemsAsync error: {ex.Message}");
            return Result<IEnumerable<CartItem>>.Failure($"Failed to get cart items: {ex.Message}");
        }
    }

    public async Task<Result<GroupedCartResponse>> GetCartItemsGroupedAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Got {items.Count} grouped items for user {userId}");

            // Group items by sales agent
            var groupedByAgent = items
                .Where(i => i.SalesAgentId.HasValue)
                .GroupBy(i => new { AgentId = i.SalesAgentId!.Value, i.SalesAgentName })
                .Select(g => new SalesAgentCartGroup
                {
                    SalesAgentId = g.Key.AgentId,
                    SalesAgentFullName = g.Key.SalesAgentName,
                    Items = g.Select(item => new CartItemResponse
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        CategoryName = item.CategoryName ?? "",
                        Price = item.Price,
                        Quantity = item.Quantity,
                        ProductImage = item.ProductImage,
                        StockAvailable = item.StockAvailable,
                        SalesAgentId = item.SalesAgentId,
                        SalesAgentUsername = item.SalesAgentName
                    }).ToList(),
                    Subtotal = g.Sum(i => i.Price * i.Quantity),
                    Tax = Math.Round(g.Sum(i => i.Price * i.Quantity) * 0.1m, 0),
                    ShippingFee = 50000,
                    ItemCount = g.Sum(i => i.Quantity)
                })
                .ToList();

            // Calculate totals for each group
            foreach (var group in groupedByAgent)
            {
                group.Total = group.Subtotal + group.Tax + group.ShippingFee;
            }

            var response = new GroupedCartResponse
            {
                UserId = userId,
                SalesAgentGroups = groupedByAgent,
                GrandTotal = groupedByAgent.Sum(g => g.Total),
                TotalItemCount = groupedByAgent.Sum(g => g.ItemCount),
                TotalSalesAgents = groupedByAgent.Count
            };

            return Result<GroupedCartResponse>.Success(response);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartItemsGroupedAsync error: {ex.Message}");
            return Result<GroupedCartResponse>.Failure($"Failed to get grouped cart items: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddToCartAsync(Guid userId, Guid productId, int quantity = 1)
    {
        try
        {
            // Get product details for validation
            var productResult = await _productRepository.GetByIdAsync(productId);
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Product {productId} not found");
                return Result<bool>.Failure("Product not found");
            }

            var product = productResult.Data;

            // Check stock
            if (product.Quantity < quantity)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Insufficient stock for {product.Name}");
                return Result<bool>.Failure("Insufficient stock");
            }

            var cartItem = await MockCartData.AddToCartAsync(userId, productId, product.Name, product.SellingPrice, quantity);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] AddToCart result: {cartItem != null}");
            return cartItem != null
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to add item to cart");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] AddToCartAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to add to cart: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateQuantityAsync(Guid userId, Guid productId, int quantity)
    {
        try
        {
            var result = await MockCartData.UpdateCartItemAsync(userId, productId, quantity);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] UpdateQuantity result: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to update quantity");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] UpdateQuantityAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to update quantity: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        try
        {
            var result = await MockCartData.RemoveFromCartAsync(userId, productId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] RemoveFromCart result: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to remove item from cart");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] RemoveFromCartAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to remove from cart: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ClearCartAsync(Guid userId)
    {
        try
        {
            var result = await MockCartData.ClearCartAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] ClearCart result: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to clear cart");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] ClearCartAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to clear cart: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetCartCountAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);
            var count = items.Sum(item => item.Quantity);
            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartCountAsync error: {ex.Message}");
            return Result<int>.Failure($"Failed to get cart count: {ex.Message}");
        }
    }

    public async Task<Result<CartSummary>> GetCartSummaryAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);

            var subtotal = items.Sum(item => item.Subtotal);
            var itemCount = items.Sum(item => item.Quantity);

            // Calculate tax (10% VAT)
            var tax = Math.Round(subtotal * 0.10m, 0);

            // Calculate shipping fee (free if > 5,000,000 VND)
            var shippingFee = subtotal > 5000000 ? 0 : 50000;

            var total = subtotal + tax + shippingFee;

            return Result<CartSummary>.Success(new CartSummary
            {
                ItemCount = itemCount,
                Subtotal = subtotal,
                Tax = tax,
                ShippingFee = shippingFee,
                Total = total
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartSummaryAsync error: {ex.Message}");
            return Result<CartSummary>.Failure($"Failed to get cart summary: {ex.Message}");
        }
    }

    public Task<Result<Order>> CheckoutBySalesAgentAsync(CheckoutBySalesAgentRequest request)
    {
        // Mock implementation - not supported in mock mode
        return Task.FromResult(Result<Order>.Failure("Checkout not supported in mock mode. Please use API mode."));
    }
}
