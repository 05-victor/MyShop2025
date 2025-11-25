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

    public async Task<Result<DashboardSummary>> GetSummaryAsync()
    {
        try
        {
            var summary = await MockDashboardData.GetDashboardSummaryAsync();
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetSummaryAsync success");
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
            // MockDashboardData doesn't implement revenue chart yet, return empty data
            var chartData = new RevenueChartData
            {
                Labels = new List<string>(),
                Data = new List<decimal>()
            };
            
            await Task.CompletedTask;
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetRevenueChartAsync not yet implemented");
            return Result<RevenueChartData>.Success(chartData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] GetRevenueChartAsync error: {ex.Message}");
            return Result<RevenueChartData>.Failure($"Failed to load revenue chart: {ex.Message}", ex);
        }
    }
}
