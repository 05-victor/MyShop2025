using MyShop.Data.Entities;
using MyShop.Server.Factories.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Enums;
using MyShop.Shared.Extensions;

namespace MyShop.Server.Factories.Implementations;

/// <summary>
/// Factory for creating Order entities from CreateOrderRequest
/// </summary>
public class OrderFactory : BaseFactory<Order, CreateOrderRequest>, IOrderFactory
{
    /// <summary>
    /// Create a new Order entity from a CreateOrderRequest
    /// </summary>
    /// <param name="request">The order creation request</param>
    /// <returns>A new Order entity with initialized fields</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public override Order Create(CreateOrderRequest request)
    {
        // Basic validation
        //if (request.GrandTotal < 0)
        //    throw new ArgumentException("Grand total cannot be negative.", nameof(request.GrandTotal));

        //if (request.TotalAmount < 0)
        //    throw new ArgumentException("Total amount cannot be negative.", nameof(request.TotalAmount));

        if (request.DiscountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(request.DiscountAmount));

        // Parse status strings to enums
        var status = StatusEnumExtensions.ParseApiString<OrderStatus>(request.Status);
        var paymentStatus = StatusEnumExtensions.ParseApiString<PaymentStatus>(request.PaymentStatus);

        // Initialize new Order entity
        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            Status = status,
            PaymentStatus = paymentStatus,
            //TotalAmount = request.TotalAmount,
            DiscountAmount = request.DiscountAmount,
            ShippingFee = request.ShippingFee,
            TaxAmount = request.TaxAmount,
            //GrandTotal = request.GrandTotal,
            Note = request.Note?.Trim(),
            CustomerId = request.CustomerId,
            SaleAgentId = request.SaleAgentId ?? Guid.Empty
        };

        // Create order items if provided
        if (request.OrderItems != null && request.OrderItems.Any())
        {
            order.OrderItems = request.OrderItems.Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitSalePrice = item.UnitSalePrice,
                TotalPrice = item.UnitSalePrice * item.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Order = order
            }).ToList();
        }
        else
        {
            order.OrderItems = new List<OrderItem>();
        }

        // Calculate TotalAmount and GrandTotal
        order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);

        order.GrandTotal = order.TotalAmount - order.DiscountAmount + order.ShippingFee + order.TaxAmount;

        // Set additional fields
        AssignNewId(order);
        SetAuditFields(order);
        
        return order;
    }
}
