using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Server.Factories.Interfaces;

/// <summary>
/// Factory interface for creating Order entities from requests
/// </summary>
public interface IOrderFactory
{
    /// <summary>
    /// Create a new Order entity from a CreateOrderRequest
    /// </summary>
    /// <param name="request">The order creation request containing order details</param>
    /// <returns>A new Order entity with initialized fields</returns>
    /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
    Order Create(CreateOrderRequest request);
}
