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
}
