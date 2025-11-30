using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Shared.Adapters;

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
            OrderCode = dto.OrderNumber,
            SalesAgentId = dto.SalesAgentId,
            SalesAgentName = dto.SalesAgentName,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            CustomerAddress = dto.ShippingAddress != null
                ? string.Join(", ", new[] { dto.ShippingAddress.Street, dto.ShippingAddress.District, dto.ShippingAddress.City, dto.ShippingAddress.PostalCode }.Where(s => !string.IsNullOrWhiteSpace(s)))
                : dto.CustomerEmail,
            Status = dto.Status,
            FinalPrice = dto.TotalAmount,
            Subtotal = dto.TotalAmount, // May need adjustment if backend provides subtotal
            Notes = dto.Notes,
            OrderDate = dto.CreatedAt,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Items = dto.Items.Select(ToOrderItemModel).ToList(),
            OrderItems = dto.Items.Select(ToOrderItemModel).ToList()
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
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Total = dto.Subtotal,
            TotalPrice = dto.Subtotal
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
