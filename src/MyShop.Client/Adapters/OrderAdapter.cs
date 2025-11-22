using MyShop.Shared.Models;

namespace MyShop.Client.Adapters;

/// <summary>
/// Adapter for mapping Order DTOs to Order Models (SKELETON)
/// TODO: Implement when backend provides OrderResponse DTO
/// </summary>
public static class OrderAdapter
{
    /// <summary>
    /// OrderResponse (DTO) → Order (Model)
    /// </summary>
    public static Order ToModel(dynamic dto)
    {
        throw new NotImplementedException("Backend OrderResponse DTO not ready. Waiting for backend team.");
    }

    /// <summary>
    /// Convert list of OrderResponse to list of Order
    /// </summary>
    public static List<Order> ToModelList(IEnumerable<dynamic> dtos)
    {
        throw new NotImplementedException("Backend OrderResponse DTO not ready. Waiting for backend team.");
    }
}
