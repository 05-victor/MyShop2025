using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Adapters;

/// <summary>
/// Adapter for mapping Order DTOs to Order Models
/// Static class for stateless DTO-to-Model transformations
/// </summary>
public static class OrderAdapter
{
    /// <summary>
    /// OrderResponse (DTO) → Order (Model)
    /// </summary>
    public static Order ToModel(OrderResponse dto)
    {
        return new Order
        {
            Id = dto.Id,
            OrderCode = $"ORD-{dto.Id.ToString()[..8]}", // Generate order code from ID
            SalesAgentId = dto.SaleAgentId,
            SalesAgentName = dto.SaleAgentFullName ?? dto.SaleAgentUsername,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerFullName ?? dto.CustomerUsername ?? dto.CustomerEmail ?? "Unknown",
            CustomerAddress = dto.CustomerEmail, // Use email as fallback since ShippingAddress is not in DTO
            Status = dto.Status ?? "PENDING",
            FinalPrice = dto.GrandTotal,
            Subtotal = dto.TotalAmount,
            Discount = dto.DiscountAmount,
            Notes = dto.Note,
            OrderDate = dto.OrderDate,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Items = dto.OrderItems?.Select(ToOrderItemModel).ToList() ?? new List<OrderItem>(),
            OrderItems = dto.OrderItems?.Select(ToOrderItemModel).ToList() ?? new List<OrderItem>()
        };
    }

    /// <summary>
    /// OrderItemResponse (DTO) → OrderItem (Model)
    /// </summary>
    public static OrderItem ToOrderItemModel(OrderItemResponse dto)
    {
        return new OrderItem
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName ?? "Unknown Product",
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitSalePrice,
            Total = dto.TotalPrice,
            TotalPrice = dto.TotalPrice
        };
    }

    /// <summary>
    /// Convert list of OrderResponse to list of Order
    /// </summary>
    public static List<Order> ToModelList(IEnumerable<OrderResponse> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }
}