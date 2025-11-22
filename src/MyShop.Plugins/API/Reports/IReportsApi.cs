using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Reports;

/// <summary>
/// Refit interface for Reports API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IReportsApi
{
    [Get("/api/v1/reports/sales")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SalesReportResponse>>> GetSalesReportAsync([Query] DateTime? from, [Query] DateTime? to);

    [Get("/api/v1/reports/revenue")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SalesReportResponse>>> GetRevenueReportAsync([Query] DateTime? from, [Query] DateTime? to);

    [Get("/api/v1/reports/products")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<List<ProductPerformanceResponse>>>> GetProductReportAsync();
}
