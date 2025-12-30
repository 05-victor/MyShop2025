using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Plugins.API.Dashboard;
using MyShop.Shared.Models;
using MyShop.Shared.DTOs.Responses;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.Facades.Reports;

/// <summary>
/// Facade for reporting operations
/// Aggregates: IReportRepository, IDashboardApi, IToastService
/// </summary>
public class ReportFacade : IReportFacade
{
    private readonly IReportRepository _reportRepository;
    private readonly IDashboardApi _dashboardApi;
    private readonly IToastService _toastService;

    public ReportFacade(
        IReportRepository reportRepository,
        IDashboardApi dashboardApi,
        IToastService toastService)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _dashboardApi = dashboardApi ?? throw new ArgumentNullException(nameof(dashboardApi));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<SalesReport>> GetSalesReportAsync(string period = "current")
    {
        try
        {
            return Result<SalesReport>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error getting sales report: {ex.Message}");
            return Result<SalesReport>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProductPerformance>>> GetProductPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, int top = 20)
    {
        try
        {
            return Result<List<ProductPerformance>>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error getting product performance: {ex.Message}");
            return Result<List<ProductPerformance>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<AgentPerformance>>> GetAgentPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            return Result<List<AgentPerformance>>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error getting agent performance: {ex.Message}");
            return Result<List<AgentPerformance>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SalesTrend>> GetSalesTrendAsync(string period = "current")
    {
        try
        {
            return Result<SalesTrend>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error getting sales trend: {ex.Message}");
            return Result<SalesTrend>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PerformanceMetrics>> GetPerformanceMetricsAsync(string period = "current")
    {
        try
        {
            return Result<PerformanceMetrics>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error getting performance metrics: {ex.Message}");
            return Result<PerformanceMetrics>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportSalesReportAsync(string period = "current")
    {
        try
        {
            // Show file save picker first
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV Files", new List<string> { ".csv" });
            savePicker.SuggestedFileName = $"SalesReport_{period}_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                // User cancelled the picker
                return Result<string>.Success(string.Empty);
            }

            var reportResult = await GetSalesReportAsync(period);

            var csv = new StringBuilder();
            csv.AppendLine("SALES REPORT");
            csv.AppendLine($"Period,{period}");
            csv.AppendLine($"Export Date,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine();

            if (reportResult.IsSuccess && reportResult.Data != null)
            {
                var report = reportResult.Data;
                csv.AppendLine("SUMMARY");
                csv.AppendLine("Metric,Value");
                csv.AppendLine($"Total Revenue,\"{report.TotalRevenue:F2}\"");
                csv.AppendLine($"Total Orders,\"{report.TotalOrders}\"");
                csv.AppendLine($"Average Order Value,\"{report.AverageOrderValue:F2}\"");
            }
            else
            {
                csv.AppendLine("SUMMARY");
                csv.AppendLine("Note,No data available for this period");
            }

            // Write to user-selected file
            await FileIO.WriteTextAsync(file, csv.ToString());

            await _toastService.ShowSuccess($"Sales report exported to {file.Name}");
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Exported sales report to {file.Path}");

            return Result<string>.Success(file.Path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error exporting sales report: {ex.Message}");
            await _toastService.ShowError($"Error exporting report: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportProductPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // Show file save picker first
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV Files", new List<string> { ".csv" });
            savePicker.SuggestedFileName = $"ProductPerformance_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                // User cancelled the picker
                return Result<string>.Success(string.Empty);
            }

            var performanceResult = await GetProductPerformanceAsync(startDate, endDate, top: 1000);

            var csv = new StringBuilder();
            csv.AppendLine("PRODUCT PERFORMANCE REPORT");
            csv.AppendLine($"Date Range,{startDate?.ToString("yyyy-MM-dd") ?? "All"} to {endDate?.ToString("yyyy-MM-dd") ?? "All"}");
            csv.AppendLine($"Export Date,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine();

            csv.AppendLine("Product Name,Category,Units Sold,Revenue,Commission,Clicks,Conversion Rate");

            if (performanceResult.IsSuccess && performanceResult.Data != null)
            {
                foreach (var product in performanceResult.Data)
                {
                    csv.AppendLine($"\"{product.ProductName}\",\"{product.CategoryName ?? "N/A"}\"," +
                        $"\"{product.TotalSold}\",\"{product.TotalRevenue:F2}\"," +
                        $"\"{product.TotalCommission:F2}\",\"{product.Clicks}\",\"{product.ConversionRate:F2}%\"");
                }
            }

            // Write to user-selected file
            await FileIO.WriteTextAsync(file, csv.ToString());

            await _toastService.ShowSuccess($"Product performance exported to {file.Name}");
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Exported product performance to {file.Path}");

            return Result<string>.Success(file.Path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Error exporting product performance: {ex.Message}");
            await _toastService.ShowError($"Error exporting performance: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<AdminReportsResponse>> GetAdminReportsAsync(
        DateTime from,
        DateTime to,
        Guid? categoryId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            // Validate date range
            if (from > to)
            {
                return Result<AdminReportsResponse>.Failure("Start date must be before end date");
            }

            // Convert to UTC if not already UTC
            var fromUtc = from.Kind == DateTimeKind.Utc ? from : from.ToUniversalTime();
            var toUtc = to.Kind == DateTimeKind.Utc ? to : to.ToUniversalTime();

            // Serialize to ISO 8601 UTC format with 'Z' suffix
            var fromString = fromUtc.ToString("O"); // "O" format: 2025-12-21T18:04:58.0000000Z
            var toString = toUtc.ToString("O");

            System.Diagnostics.Debug.WriteLine(
                $"[ReportFacade] Calling GetAdminReportsAsync: from={fromString} (Kind={fromUtc.Kind}), to={toString} (Kind={toUtc.Kind}), categoryId={categoryId}, page={pageNumber}, size={pageSize}");

            // Call API with string parameters to ensure proper serialization
            var response = await _dashboardApi.GetAdminReportsAsync(fromString, toString, categoryId, pageNumber, pageSize);

            if (response.IsSuccessStatusCode && response.Content?.Result != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ReportFacade] API Success: {response.Content.Result.ProductSummary?.Data?.Count ?? 0} products loaded");
                return Result<AdminReportsResponse>.Success(response.Content.Result);
            }

            var errorMsg = response.Content?.Message ?? $"API returned {response.StatusCode}";
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] API Error: {errorMsg}");
            return Result<AdminReportsResponse>.Failure(errorMsg);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Exception in GetAdminReportsAsync: {ex.Message}");
            return Result<AdminReportsResponse>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get sales agent personal reports (revenue trend, orders by category, top products)
    /// </summary>
    public async Task<Result<SalesAgentReportsResponse>> GetSalesAgentReportsAsync(string period = "week", Guid? categoryId = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Calling GetSalesAgentReportsAsync: period={period}, categoryId={categoryId}");

            var response = await _dashboardApi.GetSalesAgentReportsAsync(period, categoryId);

            if (response.IsSuccessStatusCode && response.Content?.Result != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ReportFacade] ✅ GetSalesAgentReportsAsync Success: " +
                    $"RevenueTrend={response.Content.Result.RevenueTrend?.Count ?? 0}, " +
                    $"OrdersByCategory={response.Content.Result.OrdersByCategory?.Count ?? 0}, " +
                    $"TopProducts={response.Content.Result.TopProducts?.Count ?? 0}");
                return Result<SalesAgentReportsResponse>.Success(response.Content.Result);
            }

            var errorMsg = response.Content?.Message ?? $"API returned {response.StatusCode}";
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] API Error: {errorMsg}");
            return Result<SalesAgentReportsResponse>.Failure(errorMsg);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportFacade] Exception in GetSalesAgentReportsAsync: {ex.Message}");
            return Result<SalesAgentReportsResponse>.Failure($"Error: {ex.Message}");
        }
    }
}
