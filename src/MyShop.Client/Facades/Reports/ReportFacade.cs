using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.Facades.Reports;

/// <summary>
/// Facade for reporting operations
/// Aggregates: IReportRepository, IToastService
/// </summary>
public class ReportFacade : IReportFacade
{
    private readonly IReportRepository _reportRepository;
    private readonly IToastService _toastService;

    public ReportFacade(IReportRepository reportRepository, IToastService toastService)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
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
}
