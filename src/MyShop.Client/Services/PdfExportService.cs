using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Client.Services;

public interface IPdfExportService
{
    Task ExportSalesReportAsync(DateTime startDate, DateTime endDate, List<SalesReportData> data);
    Task ExportOrderReportAsync(List<OrderResponse> orders, string reportTitle);
    Task ExportInventoryReportAsync(List<ProductResponse> products, string reportTitle);
    Task ExportAgentCommissionReportAsync(int agentId, string agentName, List<CommissionData> data, DateTime period);
    Task ExportCustomReportAsync(string title, string subtitle, object data);
    Task<string?> ExportUsersReportAsync(IEnumerable<UserInfoResponse> users, string reportTitle);
    Task<string?> ExportDashboardReportAsync(DashboardExportData data);
}

public class PdfExportService : IPdfExportService
{
    public PdfExportService()
    {
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // Company branding constants
    private const string CompanyName = "MyShop2025";
    private const string CompanyAddress = "123 Business St, Commerce City";
    private const string CompanyPhone = "+84 123 456 789";
    private const string CompanyEmail = "info@myshop2025.com";
    private const string CompanyWebsite = "www.myshop2025.com";

    public async Task ExportSalesReportAsync(DateTime startDate, DateTime endDate, List<SalesReportData> data)
    {
        var file = await PickSaveFileAsync("Sales_Report", "PDF Document", ".pdf");
        if (file == null) return;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);

                    // Header with company branding
                    page.Header().Element(ComposeHeader);

                    // Content
                    page.Content().Column(col =>
                    {
                        // Report title
                        col.Item().PaddingBottom(20).Column(titleCol =>
                        {
                            titleCol.Item().Text("SALES REPORT").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            titleCol.Item().Text($"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                                .FontSize(14).FontColor(Colors.Grey.Darken1);
                        });

                        // Summary statistics
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Sales", 
                                $"{data.Sum(x => x.TotalAmount):C0}", Colors.Blue.Lighten3));
                            row.RelativeItem().PaddingHorizontal(10).Element(c => ComposeSummaryCard(c, "Orders", 
                                $"{data.Sum(x => x.OrderCount)}", Colors.Green.Lighten3));
                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Avg Order", 
                                $"{data.Average(x => x.AverageOrderValue):C0}", Colors.Orange.Lighten3));
                        });

                        // Sales data table
                        col.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80); // Date
                                columns.RelativeColumn(2); // Category
                                columns.RelativeColumn(1); // Orders
                                columns.RelativeColumn(1); // Items
                                columns.RelativeColumn(2); // Revenue
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date").Bold();
                                header.Cell().Element(CellStyle).Text("Category").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Orders").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Items").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Revenue").Bold();
                            });

                            // Data rows
                            foreach (var item in data)
                            {
                                table.Cell().Element(CellStyle).Text(item.Date.ToString("MMM dd"));
                                table.Cell().Element(CellStyle).Text(item.Category ?? "All");
                                table.Cell().Element(CellStyle).AlignRight().Text(item.OrderCount.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.ItemsSold.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalAmount:C0}");
                            }
                        });
                    });

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Sales report exported: {file.Path}");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export sales report", ex);
            throw;
        }
    }

    public async Task ExportOrderReportAsync(List<OrderResponse> orders, string reportTitle)
    {
        var file = await PickSaveFileAsync("Order_Report", "PDF Document", ".pdf");
        if (file == null) return;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Landscape for more columns
                    page.Margin(40);

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(20).Text(reportTitle).FontSize(24).Bold();

                        // Orders table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60); // Order ID
                                columns.ConstantColumn(80); // Date
                                columns.RelativeColumn(2); // Customer
                                columns.RelativeColumn(1); // Items
                                columns.RelativeColumn(1); // Status
                                columns.RelativeColumn(1); // Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Order ID").Bold();
                                header.Cell().Element(CellStyle).Text("Date").Bold();
                                header.Cell().Element(CellStyle).Text("Customer").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Items").Bold();
                                header.Cell().Element(CellStyle).Text("Status").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Total").Bold();
                            });

                            foreach (var order in orders)
                            {
                                table.Cell().Element(CellStyle).Text($"#{order.Id}");
                                table.Cell().Element(CellStyle).Text(order.OrderDate.ToString("MMM dd, yyyy"));
                                table.Cell().Element(CellStyle).Text(order.CustomerFullName ?? "N/A");
                                table.Cell().Element(CellStyle).AlignRight().Text(order.OrderItems?.Count.ToString() ?? "0");
                                table.Cell().Element(CellStyle).Text(order.Status ?? "Unknown");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{order.TotalAmount:C0}");
                            }
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Order report exported: {file.Path}");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export order report", ex);
            throw;
        }
    }

    public async Task ExportInventoryReportAsync(List<ProductResponse> products, string reportTitle)
    {
        var file = await PickSaveFileAsync("Inventory_Report", "PDF Document", ".pdf");
        if (file == null) return;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(20).Column(titleCol =>
                        {
                            titleCol.Item().Text(reportTitle).FontSize(24).Bold();
                            titleCol.Item().Text($"Total Products: {products.Count}").FontSize(12).FontColor(Colors.Grey.Darken1);
                        });

                        // Summary
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            var totalValue = products.Sum(p => (p.SellingPrice ?? 0) * (p.Quantity ?? 0));
                            var lowStock = products.Count(p => (p.Quantity ?? 0) < 10);
                            var outOfStock = products.Count(p => (p.Quantity ?? 0) == 0);

                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Value", $"{totalValue:C0}", Colors.Blue.Lighten3));
                            row.RelativeItem().PaddingHorizontal(10).Element(c => ComposeSummaryCard(c, "Low Stock", lowStock.ToString(), Colors.Orange.Lighten3));
                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Out of Stock", outOfStock.ToString(), Colors.Red.Lighten3));
                        });

                        // Products table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50); // ID
                                columns.RelativeColumn(3); // Name
                                columns.RelativeColumn(1); // Category
                                columns.RelativeColumn(1); // Price
                                columns.RelativeColumn(1); // Stock
                                columns.RelativeColumn(1); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("ID").Bold();
                                header.Cell().Element(CellStyle).Text("Product Name").Bold();
                                header.Cell().Element(CellStyle).Text("Category").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Price").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Stock").Bold();
                                header.Cell().Element(CellStyle).Text("Status").Bold();
                            });

                            foreach (var product in products)
                            {
                                var quantity = product.Quantity ?? 0;
                                var stockStatus = quantity == 0 ? "Out" : 
                                                 quantity < 10 ? "Low" : "OK";
                                var statusColor = quantity == 0 ? Colors.Red.Medium :
                                                 quantity < 10 ? Colors.Orange.Medium : Colors.Green.Medium;

                                table.Cell().Element(CellStyle).Text(product.Id.ToString());
                                table.Cell().Element(CellStyle).Text(product.Name);
                                table.Cell().Element(CellStyle).Text(product.CategoryName ?? "N/A");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{product.SellingPrice:C0}");
                                table.Cell().Element(CellStyle).AlignRight().Text(quantity.ToString());
                                table.Cell().Element(CellStyle).Text(stockStatus).FontColor(statusColor);
                            }
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Inventory report exported: {file.Path}");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export inventory report", ex);
            throw;
        }
    }

    public async Task ExportAgentCommissionReportAsync(int agentId, string agentName, List<CommissionData> data, DateTime period)
    {
        var file = await PickSaveFileAsync($"Commission_Report_Agent{agentId}", "PDF Document", ".pdf");
        if (file == null) return;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(20).Column(titleCol =>
                        {
                            titleCol.Item().Text("SALES AGENT COMMISSION REPORT").FontSize(24).Bold();
                            titleCol.Item().Text($"Agent: {agentName} (ID: {agentId})").FontSize(14);
                            titleCol.Item().Text($"Period: {period:MMMM yyyy}").FontSize(12).FontColor(Colors.Grey.Darken1);
                        });

                        // Commission summary
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            var totalSales = data.Sum(x => x.SalesAmount);
                            var totalCommission = data.Sum(x => x.CommissionAmount);
                            var commissionRate = data.FirstOrDefault()?.CommissionRate ?? 0;

                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Sales", $"{totalSales:C0}", Colors.Blue.Lighten3));
                            row.RelativeItem().PaddingHorizontal(10).Element(c => ComposeSummaryCard(c, "Commission Rate", $"{commissionRate}%", Colors.Purple.Lighten3));
                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Commission", $"{totalCommission:C0}", Colors.Green.Lighten3));
                        });

                        // Commission details table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80); // Date
                                columns.ConstantColumn(80); // Order ID
                                columns.RelativeColumn(2); // Customer
                                columns.RelativeColumn(1); // Sales
                                columns.RelativeColumn(1); // Rate
                                columns.RelativeColumn(1); // Commission
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date").Bold();
                                header.Cell().Element(CellStyle).Text("Order ID").Bold();
                                header.Cell().Element(CellStyle).Text("Customer").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Sales").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Rate").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Commission").Bold();
                            });

                            foreach (var item in data)
                            {
                                table.Cell().Element(CellStyle).Text(item.Date.ToString("MMM dd"));
                                table.Cell().Element(CellStyle).Text($"#{item.OrderId}");
                                table.Cell().Element(CellStyle).Text(item.CustomerName);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.SalesAmount:C0}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.CommissionRate}%");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.CommissionAmount:C0}");
                            }
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Commission report exported: {file.Path}");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export commission report", ex);
            throw;
        }
    }

    public async Task ExportCustomReportAsync(string title, string subtitle, object data)
    {
        var file = await PickSaveFileAsync("Custom_Report", "PDF Document", ".pdf");
        if (file == null) return;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().Text(title).FontSize(24).Bold();
                        if (!string.IsNullOrEmpty(subtitle))
                        {
                            col.Item().Text(subtitle).FontSize(14).FontColor(Colors.Grey.Darken1);
                        }

                        col.Item().PaddingVertical(20).Text(data?.ToString() ?? "No data available");
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Custom report exported: {file.Path}");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export custom report", ex);
            throw;
        }
    }

    // Helper methods
    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(brandCol =>
                {
                    brandCol.Item().Text(CompanyName).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    brandCol.Item().Text(CompanyAddress).FontSize(9).FontColor(Colors.Grey.Medium);
                    brandCol.Item().Text($"{CompanyPhone} | {CompanyEmail}").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(100).AlignRight().Text(CompanyWebsite).FontSize(9).FontColor(Colors.Blue.Medium);
            });

            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Blue.Medium);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(9).FontColor(Colors.Grey.Medium);
                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private void ComposeSummaryCard(IContainer container, string label, string value, string backgroundColor)
    {
        container.Background(backgroundColor).Padding(15).Column(col =>
        {
            col.Item().Text(label).FontSize(11).FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(5).Text(value).FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
        });
    }

    private IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }

    private async Task<StorageFile> PickSaveFileAsync(string suggestedFileName, string fileTypeLabel, string fileExtension)
    {
        var savePicker = new FileSavePicker();
        var window = App.MainWindow;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add(fileTypeLabel, new List<string>() { fileExtension });
        savePicker.SuggestedFileName = $"{SanitizeFileName(suggestedFileName)}_{DateTime.Now:yyyyMMdd_HHmmss}";

        return await savePicker.PickSaveFileAsync();
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Report";

        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Where(c => !invalidChars.Contains(c))).Trim();
    }

    /// <summary>
    /// Export users list to PDF with simple table layout
    /// </summary>
    public async Task<string?> ExportUsersReportAsync(IEnumerable<UserInfoResponse> users, string reportTitle)
    {
        var file = await PickSaveFileAsync("Users_Report", "PDF Document", ".pdf");
        if (file == null) return null;

        try
        {
            var userList = users.ToList();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(40);
                    page.PageColor(Colors.White);

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(20).Column(titleCol =>
                        {
                            titleCol.Item().Text(reportTitle).FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            titleCol.Item().Text($"Total Users: {userList.Count}").FontSize(14).FontColor(Colors.Grey.Darken1);
                            titleCol.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Username
                                columns.RelativeColumn(2); // Full Name
                                columns.RelativeColumn(3); // Email
                                columns.RelativeColumn(2); // Phone
                                columns.RelativeColumn(1); // Role
                                columns.RelativeColumn(1); // Verified
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Username").Bold();
                                header.Cell().Element(CellStyle).Text("Full Name").Bold();
                                header.Cell().Element(CellStyle).Text("Email").Bold();
                                header.Cell().Element(CellStyle).Text("Phone").Bold();
                                header.Cell().Element(CellStyle).Text("Role").Bold();
                                header.Cell().Element(CellStyle).Text("Verified").Bold();
                            });

                            foreach (var user in userList)
                            {
                                var role = user.RoleNames?.FirstOrDefault() ?? "Customer";
                                table.Cell().Element(CellStyle).Text(user.Username ?? "N/A");
                                table.Cell().Element(CellStyle).Text(user.FullName ?? "N/A");
                                table.Cell().Element(CellStyle).Text(user.Email ?? "N/A");
                                table.Cell().Element(CellStyle).Text(user.PhoneNumber ?? "N/A");
                                table.Cell().Element(CellStyle).Text(role);
                                table.Cell().Element(CellStyle).Text(user.IsEmailVerified ? "Yes" : "No");
                            }
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Users report exported: {file.Path}");
            return file.Path;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export users report", ex);
            throw;
        }
    }

    /// <summary>
    /// Export dashboard data to PDF
    /// </summary>
    public async Task<string?> ExportDashboardReportAsync(DashboardExportData data)
    {
        var file = await PickSaveFileAsync("Dashboard_Report", "PDF Document", ".pdf");
        if (file == null) return null;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(20).Column(titleCol =>
                        {
                            titleCol.Item().Text("Admin Dashboard Report").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                            titleCol.Item().Text($"Period: {data.Period}").FontSize(14).FontColor(Colors.Grey.Darken1);
                            titleCol.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        // KPI Summary
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total GMV", $"{data.TotalGmv:C0}", Colors.Blue.Lighten3));
                            row.RelativeItem().PaddingHorizontal(10).Element(c => ComposeSummaryCard(c, "Commission", $"{data.AdminCommission:C0}", Colors.Green.Lighten3));
                            row.RelativeItem().PaddingHorizontal(10).Element(c => ComposeSummaryCard(c, "Active Agents", $"{data.ActiveAgents}", Colors.Orange.Lighten3));
                            row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Products", $"{data.TotalProducts}", Colors.Purple.Lighten3));
                        });

                        // Top Sales Agents
                        if (data.TopAgents?.Any() == true)
                        {
                            col.Item().PaddingTop(20).Text("Top Sales Agents").FontSize(16).Bold();
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Name
                                    columns.RelativeColumn(2); // Email
                                    columns.RelativeColumn(1); // GMV
                                    columns.RelativeColumn(1); // Commission
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Agent").Bold();
                                    header.Cell().Element(CellStyle).Text("Email").Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("GMV").Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Commission").Bold();
                                });

                                foreach (var agent in data.TopAgents)
                                {
                                    table.Cell().Element(CellStyle).Text(agent.Name ?? "N/A");
                                    table.Cell().Element(CellStyle).Text(agent.Email ?? "N/A");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{agent.GMV:C0}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{agent.Commission:C0}");
                                }
                            });
                        }

                        // Top Products
                        if (data.TopProducts?.Any() == true)
                        {
                            col.Item().PaddingTop(20).Text("Top Selling Products").FontSize(16).Bold();
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Name
                                    columns.RelativeColumn(1); // Sold
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Product").Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Units Sold").Bold();
                                });

                                foreach (var product in data.TopProducts)
                                {
                                    table.Cell().Element(CellStyle).Text(product.Name ?? "N/A");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{product.SoldCount}");
                                }
                            });
                        }
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            using var stream = await file.OpenStreamForWriteAsync();
            document.GeneratePdf(stream);

            LoggingService.Instance.Information($"Dashboard report exported: {file.Path}");
            return file.Path;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export dashboard report", ex);
            throw;
        }
    }
}

// Helper data models
public class SalesReportData
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public int OrderCount { get; set; }
    public int ItemsSold { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageOrderValue => OrderCount > 0 ? TotalAmount / OrderCount : 0;
}

public class CommissionData
{
    public DateTime Date { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal SalesAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
}

public class DashboardExportData
{
    public string Period { get; set; } = "Month";
    public decimal TotalGmv { get; set; }
    public decimal AdminCommission { get; set; }
    public int ActiveAgents { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public List<TopAgentExportItem> TopAgents { get; set; } = new();
    public List<TopProductExportItem> TopProducts { get; set; } = new();
}

public class TopAgentExportItem
{
    public string Name { get; set; }
    public string Email { get; set; }
    public decimal GMV { get; set; }
    public decimal Commission { get; set; }
}

public class TopProductExportItem
{
    public string Name { get; set; }
    public int SoldCount { get; set; }
}
