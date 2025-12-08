using MyShop.Shared.DTOs.Responses;
using MyShop.Data.Entities;

namespace MyShop.Server.Mappings;

/// <summary>
/// Mapper for converting Order entities to OrderResponse DTOs
/// </summary>
public class OrderMapper
{
    /// <summary>
    /// Convert an Order entity to an OrderResponse DTO
    /// </summary>
    /// <param name="order">The order entity to convert</param>
    /// <returns>OrderResponse DTO with mapped data</returns>
    public static OrderResponse ToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            TaxAmount = order.TaxAmount,
            GrandTotal = order.GrandTotal,
            Note = order.Note,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,

            // Customer information
            CustomerId = order.CustomerId,
            CustomerUsername = order.Customer?.Username,
            CustomerFullName = order.Customer?.Profile?.FullName,
            CustomerEmail = order.Customer?.Email,

            // Sale agent information
            SaleAgentId = order.SaleAgentId,
            SaleAgentUsername = order.SaleAgent?.Username,
            SaleAgentFullName = order.SaleAgent?.Profile?.FullName,

            // Order items
            OrderItems = order.OrderItems?.Select(ToOrderItemResponse).ToList()
        };
    }

    /// <summary>
    /// Convert an OrderItem entity to an OrderItemResponse DTO
    /// </summary>
    /// <param name="orderItem">The order item entity to convert</param>
    /// <returns>OrderItemResponse DTO with mapped data</returns>
    public static OrderItemResponse ToOrderItemResponse(OrderItem orderItem)
    {
        return new OrderItemResponse
        {
            Id = orderItem.Id,
            Quantity = orderItem.Quantity,
            UnitSalePrice = orderItem.UnitSalePrice,
            TotalPrice = orderItem.TotalPrice,
            CreatedAt = orderItem.CreatedAt,
            UpdatedAt = orderItem.UpdatedAt,

            // Product information
            ProductId = orderItem.ProductId,
            ProductName = orderItem.Product?.Name,
            ProductSKU = orderItem.Product?.SKU
        };
    }
}