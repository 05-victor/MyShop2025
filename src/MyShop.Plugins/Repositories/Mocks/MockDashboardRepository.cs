using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Dashboard data - delegates to MockDashboardData
/// </summary>
public class MockDashboardRepository : IDashboardRepository
{

    public async Task<Result<DashboardSummary>> GetSummaryAsync(string period = "current")
    {
        try
        {
            var summary = await MockDashboardData.GetDashboardSummaryAsync(period);
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetSummaryAsync({period}) success");
            return Result<DashboardSummary>.Success(summary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetSummaryAsync error: {ex.Message}");
            return Result<DashboardSummary>.Failure($"Failed to load dashboard data: {ex.Message}", ex);
        }
    }

    public async Task<Result<RevenueChartData>> GetRevenueChartAsync(string period = "daily")
    {
        try
        {
            var chartData = await MockDashboardData.GetRevenueChartAsync(period);
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetRevenueChartAsync({period}) success");
            return Result<RevenueChartData>.Success(chartData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetRevenueChartAsync error: {ex.Message}");
            return Result<RevenueChartData>.Failure($"Failed to load revenue chart: {ex.Message}", ex);
        }
    }

    public async Task<Result<List<TopSalesAgent>>> GetTopSalesAgentsAsync(string period = "current", int topCount = 5)
    {
        try
        {
            var topAgents = await MockDashboardData.GetTopSalesAgentsAsync(period, topCount);
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetTopSalesAgentsAsync({period}, {topCount}) success - {topAgents.Count} agents");
            return Result<List<TopSalesAgent>>.Success(topAgents);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetTopSalesAgentsAsync error: {ex.Message}");
            return Result<List<TopSalesAgent>>.Failure($"Failed to load top sales agents: {ex.Message}", ex);
        }
    }

    public async Task<Result<MyShop.Shared.DTOs.Responses.AdminDashboardSummaryResponse>> GetAdminSummaryAsync(string? period = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetAdminSummaryAsync({period}) - Using mock data");
            var mockData = new MyShop.Shared.DTOs.Responses.AdminDashboardSummaryResponse
            {
                ActiveSalesAgents = 42,
                TotalProducts = 1250,
                TotalGmv = 125000.50m,
                AdminCommission = 6250.25m,
                TotalOrders = 3500,
                TotalRevenue = 125000.50m,
                TopSellingProducts = new List<MyShop.Shared.DTOs.Responses.TopSellingProductDto>
                {
                    new MyShop.Shared.DTOs.Responses.TopSellingProductDto { Id = Guid.NewGuid(), Name = "Laptop Pro", CategoryName = "Electronics", SoldCount = 150, Revenue = 45000.00m },
                    new MyShop.Shared.DTOs.Responses.TopSellingProductDto { Id = Guid.NewGuid(), Name = "Wireless Mouse", CategoryName = "Accessories", SoldCount = 800, Revenue = 12000.00m }
                },
                TopSalesAgents = new List<MyShop.Shared.DTOs.Responses.TopSalesAgentDto>
                {
                    new MyShop.Shared.DTOs.Responses.TopSalesAgentDto { Id = Guid.NewGuid(), Name = "John Seller", TotalGmv = 25000.00m, OrderCount = 800, Commission = 1250.00m },
                    new MyShop.Shared.DTOs.Responses.TopSalesAgentDto { Id = Guid.NewGuid(), Name = "Jane Merchant", TotalGmv = 18000.00m, OrderCount = 600, Commission = 900.00m }
                }
            };
            return Result<MyShop.Shared.DTOs.Responses.AdminDashboardSummaryResponse>.Success(mockData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetAdminSummaryAsync error: {ex.Message}");
            return Result<MyShop.Shared.DTOs.Responses.AdminDashboardSummaryResponse>.Failure($"Failed to load admin summary: {ex.Message}", ex);
        }
    }

    public async Task<Result<MyShop.Shared.DTOs.Responses.AdminRevenueChartResponse>> GetAdminRevenueChartAsync(string period)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetAdminRevenueChartAsync({period}) - Using mock data");
            var labels = period switch
            {
                "day" => Enumerable.Range(0, 24).Select(h => h.ToString()).ToList(),
                "month" => Enumerable.Range(1, 31).Select(d => d.ToString()).ToList(),
                "year" => new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
                _ => new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }
            };
            var revenueData = labels.Select(_ => (decimal)(new Random().NextDouble() * 50000)).ToList();
            var commissionData = revenueData.Select(r => Math.Round(r * 0.05m, 2)).ToList();
            var mockData = new MyShop.Shared.DTOs.Responses.AdminRevenueChartResponse
            {
                Labels = labels,
                RevenueData = revenueData,
                CommissionData = commissionData
            };
            return Result<MyShop.Shared.DTOs.Responses.AdminRevenueChartResponse>.Success(mockData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetAdminRevenueChartAsync error: {ex.Message}");
            return Result<MyShop.Shared.DTOs.Responses.AdminRevenueChartResponse>.Failure($"Failed to load admin revenue chart: {ex.Message}", ex);
        }
    }
}
