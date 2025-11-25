using MyShop.Shared.Models;

namespace MyShop.Shared.Adapters;

/// <summary>
/// Adapter for mapping Dashboard DTOs to Dashboard Models
/// </summary>
public static class DashboardAdapter
{
    /// <summary>
    /// Dashboard DTO → Dashboard Model
    /// Currently Dashboard model exists in Shared.Models
    /// </summary>
    public static DashboardSummary ToModel(dynamic dto)
    {
        return new DashboardSummary
        {
            TodayRevenue = dto.TotalRevenue ?? 0m,
            TodayOrders = dto.TotalOrders ?? 0,
            TotalProducts = dto.TotalProducts ?? 0
        };
    }
}
