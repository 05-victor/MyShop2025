using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Client.Views.Order;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdminOrderPage : Page
{
    public List<string> Categories { get; } = new()
        {
            "All Categories",
            "Smartphones",
            "Laptops",
            "Audio"
        };

    public List<string> Statuses { get; } = new()
        {
            "All Status",
            "Pending",
            "Shipped",
            "Delivered",
            "Cancelled"
        };
    public ObservableCollection<ProductPerformanceItem> ProductPerformanceItems { get; } =
            new ObservableCollection<ProductPerformanceItem>();

    // Recent Orders & All Orders
    public ObservableCollection<OrderListItem> RecentOrders { get; } =
        new ObservableCollection<OrderListItem>();

    public ObservableCollection<OrderListItem> AllOrders { get; } =
        new ObservableCollection<OrderListItem>();

    public string AllOrdersSummaryText => $"Showing {AllOrders.Count} of {AllOrders.Count} orders";

    public AdminOrderPage()
    {
        this.InitializeComponent();

        // Tạm thời dùng chính page làm DataContext
        this.DataContext = this;

        LoadDummyData();
    }

    // ===== Dummy data để chạy demo =====

    private void LoadDummyData()
    {
        // Xoá dữ liệu cũ (nếu có)
        ProductPerformanceItems.Clear();
        RecentOrders.Clear();
        AllOrders.Clear();

        // Product Performance Summary (giống hình React)
        ProductPerformanceItems.Add(new ProductPerformanceItem
        {
            ProductName = "MacBook Pro 16\"",
            CategoryName = "Laptops",
            TotalOrders = 1,
            TotalQuantity = 1,
            TotalRevenue = 2499,
            AverageOrderValue = 2499
        });
        ProductPerformanceItems.Add(new ProductPerformanceItem
        {
            ProductName = "iPhone 14 Pro Max",
            CategoryName = "Smartphones",
            TotalOrders = 1,
            TotalQuantity = 2,
            TotalRevenue = 2198,
            AverageOrderValue = 2198
        });
        ProductPerformanceItems.Add(new ProductPerformanceItem
        {
            ProductName = "Samsung Galaxy S23 Ultra",
            CategoryName = "Smartphones",
            TotalOrders = 1,
            TotalQuantity = 1,
            TotalRevenue = 1199,
            AverageOrderValue = 1199
        });
        ProductPerformanceItems.Add(new ProductPerformanceItem
        {
            ProductName = "AirPods Pro 2",
            CategoryName = "Audio",
            TotalOrders = 1,
            TotalQuantity = 3,
            TotalRevenue = 747,
            AverageOrderValue = 747
        });
        ProductPerformanceItems.Add(new ProductPerformanceItem
        {
            ProductName = "Sony WH-1000XM5",
            CategoryName = "Audio",
            TotalOrders = 1,
            TotalQuantity = 1,
            TotalRevenue = 399,
            AverageOrderValue = 399
        });

        // Recent Orders (top 3)
        RecentOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-001",
            CustomerName = "John Smith",
            ProductName = "iPhone 14 Pro Max",
            Quantity = 2,
            Total = 2198,
            OrderDate = "10/28/2025",
            Status = "Delivered"
        });
        RecentOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-002",
            CustomerName = "Sarah Johnson",
            ProductName = "MacBook Pro 16\"",
            Quantity = 1,
            Total = 2499,
            OrderDate = "10/28/2025",
            Status = "Shipped"
        });
        RecentOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-003",
            CustomerName = "Michael Chen",
            ProductName = "AirPods Pro 2",
            Quantity = 3,
            Total = 747,
            OrderDate = "10/27/2025",
            Status = "Pending"
        });

        // All Orders (5 dòng)
        AllOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-001",
            CustomerName = "John Smith",
            ProductName = "iPhone 14 Pro Max",
            Quantity = 2,
            Total = 2198,
            OrderDate = "10/28/2025",
            Status = "Delivered"
        });
        AllOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-002",
            CustomerName = "Sarah Johnson",
            ProductName = "MacBook Pro 16\"",
            Quantity = 1,
            Total = 2499,
            OrderDate = "10/28/2025",
            Status = "Shipped"
        });
        AllOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-003",
            CustomerName = "Michael Chen",
            ProductName = "AirPods Pro 2",
            Quantity = 3,
            Total = 747,
            OrderDate = "10/27/2025",
            Status = "Pending"
        });
        AllOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-004",
            CustomerName = "Emma Wilson",
            ProductName = "Sony WH-1000XM5",
            Quantity = 1,
            Total = 399,
            OrderDate = "10/27/2025",
            Status = "Delivered"
        });
        AllOrders.Add(new OrderListItem
        {
            OrderCode = "ORD-005",
            CustomerName = "David Brown",
            ProductName = "Samsung Galaxy S23 Ultra",
            Quantity = 1,
            Total = 1199,
            OrderDate = "10/26/2025",
            Status = "Cancelled"
        });

        // Nếu sau này bạn convert sang MVVM chuẩn thì sẽ raise PropertyChanged cho AllOrdersSummaryText.
    }

    // ===== Handlers Click tối thiểu =====

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Export Orders",
            Content = "Export feature is not implemented yet.\n(Hiện tại chỉ là demo UI.)",
            PrimaryButtonText = "OK"
        };

        await dialog.ShowAsync();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        // Demo: load lại dummy data
        LoadDummyData();
    }
}

// ===== Các lớp model đơn giản cho UI =====

public class ProductPerformanceItem
{
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class OrderListItem
{
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string OrderDate { get; set; } = string.Empty; // dùng string cho đơn giản
    public string Status { get; set; } = string.Empty;
}

