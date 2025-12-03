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
        var summary = new DashboardSummary();

        if (dto == null) return summary;

        try
        {
            // Top-level summary fields
            summary.Date = dto.date != null ? DateTime.Parse((string)dto.date) : DateTime.UtcNow;
            summary.TotalProducts = dto.totalProducts != null ? (int)dto.totalProducts : (dto.TotalProducts != null ? (int)dto.TotalProducts : 0);
            summary.TodayOrders = dto.todayOrders != null ? (int)dto.todayOrders : (dto.TodayOrders != null ? (int)dto.TodayOrders : 0);
            summary.TodayRevenue = dto.todayRevenue != null ? (decimal)dto.todayRevenue : (dto.TodayRevenue != null ? (decimal)dto.TodayRevenue : 0m);
            summary.WeekRevenue = dto.weekRevenue != null ? (decimal)dto.weekRevenue : (dto.WeekRevenue != null ? (decimal)dto.WeekRevenue : 0m);
            summary.MonthRevenue = dto.monthRevenue != null ? (decimal)dto.monthRevenue : (dto.MonthRevenue != null ? (decimal)dto.MonthRevenue : 0m);

            // Low stock products
            if (dto.lowStockProducts != null)
            {
                foreach (var p in dto.lowStockProducts)
                {
                    summary.LowStockProducts.Add(new LowStockProduct
                    {
                        Id = p.id != null ? Guid.Parse((string)p.id) : Guid.Empty,
                        Name = p.name ?? string.Empty,
                        CategoryName = p.categoryName,
                        Quantity = p.quantity != null ? (int)p.quantity : 0,
                        ImageUrl = p.imageUrl,
                        Status = p.status ?? string.Empty
                    });
                }
            }

            // Top selling
            if (dto.topSellingProducts != null)
            {
                foreach (var p in dto.topSellingProducts)
                {
                    summary.TopSellingProducts.Add(new TopSellingProduct
                    {
                        Id = p.id != null ? Guid.Parse((string)p.id) : Guid.Empty,
                        Name = p.name ?? string.Empty,
                        CategoryName = p.categoryName,
                        SoldCount = p.soldCount != null ? (int)p.soldCount : 0,
                        Revenue = p.revenue != null ? (decimal)p.revenue : 0m,
                        ImageUrl = p.imageUrl
                    });
                }
            }

            // Recent orders
            if (dto.recentOrders != null)
            {
                foreach (var o in dto.recentOrders)
                {
                    DateTime od = o.orderDate != null ? DateTime.Parse((string)o.orderDate) : DateTime.UtcNow;
                    summary.RecentOrders.Add(new RecentOrder
                    {
                        Id = o.id != null ? Guid.Parse((string)o.id) : Guid.Empty,
                        CustomerName = o.customerName ?? string.Empty,
                        OrderDate = od,
                        TotalAmount = o.totalAmount != null ? (decimal)o.totalAmount : 0m,
                        Status = o.status ?? string.Empty,
                        SalesAgentName = o.salesAgentName ?? string.Empty
                    });
                }
            }

            // Sales by category
            if (dto.salesByCategory != null)
            {
                foreach (var c in dto.salesByCategory)
                {
                    summary.SalesByCategory.Add(new CategorySales
                    {
                        CategoryName = c.categoryName ?? string.Empty,
                        TotalRevenue = c.totalRevenue != null ? (decimal)c.totalRevenue : 0m,
                        OrderCount = c.orderCount != null ? (int)c.orderCount : 0,
                        Percentage = c.percentage != null ? (double)c.percentage : 0.0
                    });
                }
            }
        }
        catch
        {
            // Swallow any dynamic parse errors and return partial summary
        }

        return summary;
    }
}
