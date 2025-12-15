using Microsoft.Extensions.Configuration;
using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Mappings;

/// <summary>
/// Mapper for converting CartItem entities to response DTOs
/// Uses configuration for tax, shipping, etc.
/// </summary>
public class CartMapper
{
    private readonly IConfiguration _configuration;

    public CartMapper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Maps a CartItem entity to CartItemResponse DTO
    /// </summary>
    public CartItemResponse ToCartItemResponse(CartItem cartItem)
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
            AddedAt = cartItem.CreatedAt,
            SalesAgentId = cartItem.Product?.SaleAgentId,
            SalesAgentUsername = cartItem.Product?.SaleAgent?.Username,
            SalesAgentFullName = cartItem.Product?.SaleAgent?.Profile?.FullName
        };
    }

    /// <summary>
    /// Maps a collection of CartItem entities to CartResponse DTO
    /// Uses configured tax rate, shipping fee, and free shipping threshold from appsettings.json
    /// </summary>
    public CartResponse ToCartResponse(IEnumerable<CartItem> cartItems, Guid userId)
    {
        var itemResponses = cartItems.Select(ToCartItemResponse).ToList();
        var subtotal = itemResponses.Sum(i => i.Subtotal);
        
        // Read business settings from configuration
        var taxRate = _configuration.GetValue<decimal>("BusinessSettings:TaxRate", 0.1m);
        var shippingFee = _configuration.GetValue<decimal>("BusinessSettings:ShippingFee", 30000m);
        var freeShippingThreshold = _configuration.GetValue<decimal>("BusinessSettings:FreeShippingThreshold", 500000m);
        var enableFreeShipping = _configuration.GetValue<bool>("BusinessSettings:EnableFreeShipping", true);
        
        // Calculate tax
        var tax = subtotal * taxRate;
        
        // Calculate shipping fee (free if enabled and above threshold)
        var shipping = enableFreeShipping && subtotal >= freeShippingThreshold 
            ? 0 
            : shippingFee;
        
        var total = subtotal + tax + shipping;

        return new CartResponse
        {
            UserId = userId,
            Items = itemResponses,
            Subtotal = subtotal,
            Tax = tax,
            ShippingFee = shipping,
            Total = total,
            ItemCount = itemResponses.Sum(i => i.Quantity)
        };
    }

    /// <summary>
    /// Maps a collection of CartItem entities to GroupedCartResponse DTO
    /// Groups items by sales agent who published the products
    /// </summary>
    public GroupedCartResponse ToGroupedCartResponse(IEnumerable<CartItem> cartItems, Guid userId)
    {
        var cartItemsList = cartItems.ToList();
        
        // Group cart items by SalesAgentId
        var groupedBySalesAgent = cartItemsList
            .Where(ci => ci.Product?.SaleAgentId != null)
            .GroupBy(ci => new 
            { 
                SaleAgentId = ci.Product!.SaleAgentId!.Value,
                SaleAgentUsername = ci.Product.SaleAgent?.Username,
                SaleAgentFullName = ci.Product.SaleAgent?.Profile?.FullName
            });

        var salesAgentGroups = new List<SalesAgentCartGroup>();
        
        // Read business settings from configuration
        var taxRate = _configuration.GetValue<decimal>("BusinessSettings:TaxRate", 0.1m);
        var shippingFee = _configuration.GetValue<decimal>("BusinessSettings:ShippingFee", 30000m);
        var freeShippingThreshold = _configuration.GetValue<decimal>("BusinessSettings:FreeShippingThreshold", 500000m);
        var enableFreeShipping = _configuration.GetValue<bool>("BusinessSettings:EnableFreeShipping", true);

        foreach (var group in groupedBySalesAgent)
        {
            var itemResponses = group.Select(ToCartItemResponse).ToList();
            var subtotal = itemResponses.Sum(i => i.Subtotal);
            var tax = subtotal * taxRate;
            var shipping = enableFreeShipping && subtotal >= freeShippingThreshold ? 0 : shippingFee;
            var total = subtotal + tax + shipping;

            salesAgentGroups.Add(new SalesAgentCartGroup
            {
                SalesAgentId = group.Key.SaleAgentId,
                SalesAgentUsername = group.Key.SaleAgentUsername,
                SalesAgentFullName = group.Key.SaleAgentFullName,
                Items = itemResponses,
                Subtotal = subtotal,
                Tax = tax,
                ShippingFee = shipping,
                Total = total,
                ItemCount = itemResponses.Sum(i => i.Quantity)
            });
        }

        // Handle items with no sales agent (if any)
        var itemsWithoutAgent = cartItemsList
            .Where(ci => ci.Product?.SaleAgentId == null)
            .ToList();

        if (itemsWithoutAgent.Any())
        {
            var itemResponses = itemsWithoutAgent.Select(ToCartItemResponse).ToList();
            var subtotal = itemResponses.Sum(i => i.Subtotal);
            var tax = subtotal * taxRate;
            var shipping = enableFreeShipping && subtotal >= freeShippingThreshold ? 0 : shippingFee;
            var total = subtotal + tax + shipping;

            salesAgentGroups.Add(new SalesAgentCartGroup
            {
                SalesAgentId = Guid.Empty,
                SalesAgentUsername = "Unknown",
                SalesAgentFullName = "Unknown Seller",
                Items = itemResponses,
                Subtotal = subtotal,
                Tax = tax,
                ShippingFee = shipping,
                Total = total,
                ItemCount = itemResponses.Sum(i => i.Quantity)
            });
        }

        return new GroupedCartResponse
        {
            UserId = userId,
            SalesAgentGroups = salesAgentGroups.OrderByDescending(g => g.Total).ToList(),
            GrandTotal = salesAgentGroups.Sum(g => g.Total),
            TotalItemCount = salesAgentGroups.Sum(g => g.ItemCount),
            TotalSalesAgents = salesAgentGroups.Count
        };
    }
}
