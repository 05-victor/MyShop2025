# 📚 Mock Repositories - Hướng dẫn chi tiết

## 🎯 Tổng quan

Folder này chứa **Mock Repository classes** để load và quản lý dữ liệu từ JSON files. Mock repositories giúp phát triển UI mà không cần backend API.

---

## 📂 Cấu trúc Files

```
src/MyShop.Plugins/Mocks/Repositories/
├── MockAuthRepository.cs          ✅ Đã có sẵn
├── MockProductRepository.cs       ✅ MỚI TẠO
├── MockCategoryRepository.cs      ✅ MỚI TẠO
├── MockOrderRepository.cs         ✅ MỚI TẠO
├── MockProfileRepository.cs       ✅ MỚI TẠO
├── MockDashboardRepository.cs     ✅ MỚI TẠO
└── MockSettingsRepository.cs      ✅ MỚI TẠO
```

---

## 🔧 Cách hoạt động

### Pattern chung:

```csharp
public class MockXxxRepository : IXxxRepository
{
    private readonly List<Entity> _data;
    private readonly string _jsonFilePath;

    public MockXxxRepository()
    {
        // 1. Xác định đường dẫn file JSON
        _jsonFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Mocks", "Data", "Json", "xxx.json"
        );
        
        // 2. Load dữ liệu từ JSON
        _data = LoadDataFromJson();
    }

    private List<Entity> LoadDataFromJson()
    {
        // Parse JSON → Convert to C# objects
        // Handle errors gracefully
    }

    public async Task<IEnumerable<Entity>> GetAllAsync()
    {
        await Task.Delay(300); // Simulate network delay
        return _data.ToList();
    }

    // ... CRUD methods
}
```

---

## 📖 Chi tiết từng Repository

### 1. **MockProductRepository.cs**

**Interfaces:** `IProductRepository`

**Chức năng:**
- ✅ Load 10 products từ `products.json`
- ✅ CRUD operations (Create, Read, Update, Delete)
- ✅ GetLowStockAsync(threshold) - Lấy sản phẩm tồn kho thấp
- ✅ GetByCategoryAsync(categoryName) - Lọc theo danh mục
- ✅ SearchAsync(query) - Tìm kiếm theo tên/manufacturer

**Methods:**

```csharp
// Basic CRUD
Task<IEnumerable<Product>> GetAllAsync()
Task<Product?> GetByIdAsync(Guid id)
Task<Product> CreateAsync(Product product)
Task<Product> UpdateAsync(Product product)
Task<bool> DeleteAsync(Guid id)

// Advanced queries
Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
Task<IEnumerable<Product>> GetByCategoryAsync(string categoryName)
Task<IEnumerable<Product>> SearchAsync(string query)
```

**Ví dụ sử dụng:**

```csharp
var repo = new MockProductRepository();

// Get all products
var products = await repo.GetAllAsync();
foreach (var p in products)
{
    Debug.WriteLine($"Product: {p.Name} - Price: {p.SellingPrice:C}");
}

// Get low stock products
var lowStock = await repo.GetLowStockAsync(10);
Debug.WriteLine($"Low stock: {lowStock.Count()} products");

// Search
var results = await repo.SearchAsync("iPhone");
```

**Network Delays:**
- GetAllAsync: 300ms
- GetByIdAsync: 200ms
- CreateAsync: 500ms
- UpdateAsync: 400ms
- DeleteAsync: 300ms

---

### 2. **MockCategoryRepository.cs**

**Interfaces:** `ICategoryRepository`

**Chức năng:**
- ✅ Load 8 categories từ `categories.json`
- ✅ CRUD operations
- ✅ Validate before delete (không xóa nếu có products)

**Methods:**

```csharp
Task<IEnumerable<Category>> GetAllAsync()
Task<Category?> GetByIdAsync(Guid id)
Task<Category> CreateAsync(Category category)
Task<Category> UpdateAsync(Category category)
Task<bool> DeleteAsync(Guid id)
```

**Ví dụ sử dụng:**

```csharp
var repo = new MockCategoryRepository();

// Get all categories
var categories = await repo.GetAllAsync();
foreach (var c in categories)
{
    Debug.WriteLine($"Category: {c.Name} - {c.Description}");
}

// Create new category
var newCat = new Category
{
    Name = "Smart Devices",
    Description = "IoT and smart home devices"
};
var created = await repo.CreateAsync(newCat);
Debug.WriteLine($"Created: {created.Id}");
```

---

### 3. **MockOrderRepository.cs**

**Chức năng:**
- ✅ Load 8 orders + 12 order items từ `orders.json`
- ✅ CRUD operations
- ✅ GetBySalesAgentAsync - Lọc theo sales agent
- ✅ GetByStatusAsync - Lọc theo status (CREATED/PAID/CANCELLED)
- ✅ GetByDateRangeAsync - Lọc theo khoảng thời gian
- ✅ MarkAsPaidAsync - Đánh dấu đơn hàng đã thanh toán
- ✅ CancelAsync - Hủy đơn hàng với lý do
- ✅ GetTodayRevenueAsync - Doanh thu hôm nay
- ✅ GetRevenueByDateRangeAsync - Doanh thu theo khoảng thời gian

**Methods:**

```csharp
// Basic CRUD
Task<IEnumerable<Order>> GetAllAsync()
Task<Order?> GetByIdAsync(Guid id)
Task<Order> CreateAsync(Order order)
Task<Order> UpdateAsync(Order order)
Task<bool> DeleteAsync(Guid id)

// Filtering
Task<IEnumerable<Order>> GetBySalesAgentAsync(Guid salesAgentId)
Task<IEnumerable<Order>> GetByStatusAsync(string status)
Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)

// Status management
Task<bool> MarkAsPaidAsync(Guid orderId)
Task<bool> CancelAsync(Guid orderId, string reason)

// Revenue calculations
Task<decimal> GetTodayRevenueAsync()
Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate)
```

**Ví dụ sử dụng:**

```csharp
var repo = new MockOrderRepository();

// Get today's revenue
var todayRevenue = await repo.GetTodayRevenueAsync();
Debug.WriteLine($"Today's revenue: {todayRevenue:C0} VND");

// Get orders by sales agent
var agentId = Guid.Parse("00000000-0000-0000-0000-000000000002");
var agentOrders = await repo.GetBySalesAgentAsync(agentId);
Debug.WriteLine($"Agent has {agentOrders.Count()} orders");

// Mark order as paid
var orderId = Guid.Parse("30000000-0000-0000-0000-000000000003");
await repo.MarkAsPaidAsync(orderId);

// Cancel order
await repo.CancelAsync(orderId, "Customer requested cancellation");

// Get revenue for date range
var from = new DateTime(2025, 11, 1);
var to = new DateTime(2025, 11, 30);
var monthRevenue = await repo.GetRevenueByDateRangeAsync(from, to);
Debug.WriteLine($"November revenue: {monthRevenue:C0} VND");
```

---

### 4. **MockProfileRepository.cs**

**Chức năng:**
- ✅ Load 5 profiles từ `profiles.json`
- ✅ GetByUserIdAsync - Lấy profile theo user ID
- ✅ CreateAsync - Tạo profile mới
- ✅ UpdateAsync - Cập nhật profile
- ✅ DeleteAsync - Xóa profile

**Data Model:**

```csharp
public class ProfileData
{
    public Guid UserId { get; set; }
    public string? Avatar { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; }
    public string? Address { get; set; }
    public string? JobTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Ví dụ sử dụng:**

```csharp
var repo = new MockProfileRepository();

// Get profile by user ID
var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
var profile = await repo.GetByUserIdAsync(userId);

if (profile != null)
{
    Debug.WriteLine($"User: {profile.FullName}");
    Debug.WriteLine($"Email: {profile.Email}");
    Debug.WriteLine($"Phone: {profile.PhoneNumber}");
}

// Update profile
profile.Address = "New address here";
profile.JobTitle = "Senior Developer";
await repo.UpdateAsync(profile);
```

---

### 5. **MockDashboardRepository.cs**

**Chức năng:**
- ✅ GetSummaryAsync - Lấy dashboard summary (stats, low stock, top selling, recent orders)
- ✅ GetRevenueChartAsync - Lấy dữ liệu biểu đồ doanh thu (daily/weekly/monthly/yearly)

**Data Models:**

```csharp
public class DashboardSummary
{
    public DateTime Date { get; set; }
    public int TotalProducts { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<LowStockProduct> LowStockProducts { get; set; }
    public List<TopSellingProduct> TopSellingProducts { get; set; }
    public List<RecentOrder> RecentOrders { get; set; }
    public List<CategorySales> SalesByCategory { get; set; }
}

public class RevenueChartData
{
    public List<string> Labels { get; set; }  // X-axis labels
    public List<decimal> Data { get; set; }   // Y-axis values
}
```

**Ví dụ sử dụng:**

```csharp
var repo = new MockDashboardRepository();

// Get dashboard summary
var summary = await repo.GetSummaryAsync();
if (summary != null)
{
    Debug.WriteLine($"Total Products: {summary.TotalProducts}");
    Debug.WriteLine($"Today Orders: {summary.TodayOrders}");
    Debug.WriteLine($"Today Revenue: {summary.TodayRevenue:C0} VND");
    
    // Low stock alerts
    Debug.WriteLine($"\nLow Stock Products ({summary.LowStockProducts.Count}):");
    foreach (var p in summary.LowStockProducts)
    {
        Debug.WriteLine($"  - {p.Name}: {p.Quantity} units");
    }
    
    // Top selling
    Debug.WriteLine($"\nTop Selling Products ({summary.TopSellingProducts.Count}):");
    foreach (var p in summary.TopSellingProducts)
    {
        Debug.WriteLine($"  - {p.Name}: {p.SoldCount} sold, {p.Revenue:C0} VND");
    }
}

// Get revenue chart data (daily)
var dailyChart = await repo.GetRevenueChartAsync("daily");
if (dailyChart != null)
{
    Debug.WriteLine($"\nDaily Revenue Chart ({dailyChart.Labels.Count} days):");
    for (int i = 0; i < dailyChart.Labels.Count; i++)
    {
        Debug.WriteLine($"  {dailyChart.Labels[i]}: {dailyChart.Data[i]:C0} VND");
    }
}

// Get weekly chart
var weeklyChart = await repo.GetRevenueChartAsync("weekly");

// Get monthly chart
var monthlyChart = await repo.GetRevenueChartAsync("monthly");

// Get yearly chart
var yearlyChart = await repo.GetRevenueChartAsync("yearly");
```

---

### 6. **MockSettingsRepository.cs**

**Chức năng:**
- ✅ GetAppSettingsAsync - Lấy cài đặt ứng dụng của user
- ✅ UpdateAppSettingsAsync - Cập nhật cài đặt
- ✅ GetSystemSettingsAsync - Lấy cài đặt hệ thống
- ✅ GetBusinessSettingsAsync - Lấy thông tin doanh nghiệp

**Data Models:**

```csharp
public class AppSettings
{
    public Guid UserId { get; set; }
    public int PageSize { get; set; }
    public string LastOpenedPage { get; set; }
    public string Theme { get; set; }  // LIGHT/DARK
    public string Language { get; set; }  // vi/en
    public NotificationSettings? Notifications { get; set; }
    public DisplaySettings? Display { get; set; }
}

public class SystemSettings
{
    public string ApplicationName { get; set; }
    public string Version { get; set; }
    public string DefaultCurrency { get; set; }
    public double TaxRate { get; set; }
    public int TrialPeriodDays { get; set; }
    public FeatureFlags? Features { get; set; }
}

public class BusinessSettings
{
    public string StoreName { get; set; }
    public string StoreAddress { get; set; }
    public string StorePhone { get; set; }
    public string StoreEmail { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
}
```

**Ví dụ sử dụng:**

```csharp
var repo = new MockSettingsRepository();

// Get app settings
var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
var appSettings = await repo.GetAppSettingsAsync(userId);
if (appSettings != null)
{
    Debug.WriteLine($"Theme: {appSettings.Theme}");
    Debug.WriteLine($"Language: {appSettings.Language}");
    Debug.WriteLine($"Page Size: {appSettings.PageSize}");
}

// Update app settings
appSettings.Theme = "DARK";
appSettings.Language = "en";
appSettings.PageSize = 20;
await repo.UpdateAppSettingsAsync(appSettings);

// Get system settings
var sysSettings = await repo.GetSystemSettingsAsync();
if (sysSettings != null)
{
    Debug.WriteLine($"App: {sysSettings.ApplicationName} v{sysSettings.Version}");
    Debug.WriteLine($"Trial Period: {sysSettings.TrialPeriodDays} days");
    Debug.WriteLine($"Tax Rate: {sysSettings.TaxRate * 100}%");
    
    if (sysSettings.Features != null)
    {
        Debug.WriteLine($"Google Login: {sysSettings.Features.GoogleLogin}");
        Debug.WriteLine($"Email Verification: {sysSettings.Features.EmailVerification}");
    }
}

// Get business settings
var bizSettings = await repo.GetBusinessSettingsAsync();
if (bizSettings != null)
{
    Debug.WriteLine($"Store: {bizSettings.StoreName}");
    Debug.WriteLine($"Address: {bizSettings.StoreAddress}");
    Debug.WriteLine($"Phone: {bizSettings.StorePhone}");
    Debug.WriteLine($"Bank: {bizSettings.BankName} - {bizSettings.BankAccountNumber}");
}
```

---

## 🔗 Dependency Injection (Phase 2)

### Cách register trong Bootstrapper.cs:

```csharp
using Microsoft.Extensions.DependencyInjection;
using MyShop.Plugins.Mocks.Repositories;
using MyShop.Data.Repositories.Interfaces;

public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Feature flag: Use mock data or real API
        bool useMockData = true;  // Set to false when backend is ready

        if (useMockData)
        {
            // Register Mock Repositories
            services.AddSingleton<IProductRepository, MockProductRepository>();
            services.AddSingleton<ICategoryRepository, MockCategoryRepository>();
            services.AddSingleton<MockOrderRepository>();
            services.AddSingleton<MockProfileRepository>();
            services.AddSingleton<MockDashboardRepository>();
            services.AddSingleton<MockSettingsRepository>();
        }
        else
        {
            // Register Real Repositories (with Refit API clients)
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            // ... other real repositories
        }

        // Register ViewModels
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
```

### Trong App.xaml.cs:

```csharp
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        // Configure DI
        var services = new ServiceCollection();
        services.ConfigureServices();
        ServiceProvider = services.BuildServiceProvider();
    }

    // ...
}
```

---

## 📱 ViewModel Integration (Phase 3)

### Ví dụ: ProductsViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Data.Entities;
using System.Collections.ObjectModel;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;

    public ProductsViewModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var products = await _productRepository.GetAllAsync();
            Products = new ObservableCollection<Product>(products);

            Debug.WriteLine($"Loaded {Products.Count} products");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading products: {ex.Message}";
            Debug.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadLowStockAsync()
    {
        try
        {
            IsLoading = true;
            
            // Cast to MockProductRepository to access extended methods
            if (_productRepository is MockProductRepository mockRepo)
            {
                var lowStock = await mockRepo.GetLowStockAsync(10);
                Products = new ObservableCollection<Product>(lowStock);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await LoadProductsAsync();
            return;
        }

        try
        {
            IsLoading = true;

            if (_productRepository is MockProductRepository mockRepo)
            {
                var results = await mockRepo.SearchAsync(query);
                Products = new ObservableCollection<Product>(results);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Sử dụng trong Page:

```csharp
public partial class ProductsPage : Page
{
    public ProductsViewModel ViewModel { get; }

    public ProductsPage()
    {
        InitializeComponent();

        // Get ViewModel from DI
        ViewModel = App.ServiceProvider.GetRequiredService<ProductsViewModel>();
        DataContext = ViewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadProductsCommand.ExecuteAsync(null);
    }
}
```

---

## 🧪 Testing (Phase 4)

### Unit Test Example:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Plugins.Mocks.Repositories;

[TestClass]
public class MockProductRepositoryTests
{
    private MockProductRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new MockProductRepository();
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturn10Products()
    {
        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        Assert.IsNotNull(products);
        Assert.AreEqual(10, products.Count());
    }

    [TestMethod]
    public async Task GetLowStockAsync_ShouldReturnProductsWithQuantityLessThan10()
    {
        // Act
        var lowStock = await _repository.GetLowStockAsync(10);

        // Assert
        Assert.IsNotNull(lowStock);
        Assert.IsTrue(lowStock.All(p => p.Quantity < 10));
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGenerateIdAndAddProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Test Product",
            SKU = "TEST-001",
            SellingPrice = 1000000,
            Quantity = 50
        };

        // Act
        var created = await _repository.CreateAsync(newProduct);
        var all = await _repository.GetAllAsync();

        // Assert
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual(11, all.Count()); // 10 + 1 new
    }

    [TestMethod]
    public async Task SearchAsync_ShouldFindProductsByName()
    {
        // Act
        var results = await _repository.SearchAsync("iPhone");

        // Assert
        Assert.IsNotNull(results);
        Assert.IsTrue(results.Any());
        Assert.IsTrue(results.All(p => 
            p.Name!.Contains("iPhone", StringComparison.OrdinalIgnoreCase)));
    }
}
```

---

## 🎓 Bài tập thực hành

### Bài 1: Load và hiển thị Products
1. Tạo ProductsPage với ListView
2. Inject MockProductRepository vào ViewModel
3. Load products khi page load
4. Hiển thị: Name, Price, Quantity, Status

### Bài 2: Dashboard với KPIs
1. Tạo DashboardPage
2. Inject MockDashboardRepository
3. Hiển thị: Total Products, Today Revenue, Today Orders
4. Hiển thị Low Stock Products (Top 3)
5. Hiển thị Top Selling Products (Top 5)

### Bài 3: Revenue Chart
1. Sử dụng WinUI charting library
2. Load revenue chart data (daily/weekly/monthly)
3. Vẽ biểu đồ line chart
4. Cho phép user chọn period (radio buttons)

### Bài 4: Settings Page
1. Load app settings
2. Cho phép user thay đổi: Theme, Language, Page Size
3. Save settings khi user click Save
4. Apply theme ngay lập tức

### Bài 5: Order Management
1. Tạo OrdersPage với DataGrid
2. Load orders (filter by status)
3. Cho phép mark order as paid
4. Cho phép cancel order với reason
5. Hiển thị revenue statistics

---

## 📊 Performance Tips

### 1. **Network Delay Simulation**
```csharp
// Giảm delay cho development
await Task.Delay(100); // Instead of 300ms

// Hoặc tắt hẳn
// await Task.Delay(0);
```

### 2. **Lazy Loading**
```csharp
// Load data on-demand instead of constructor
private List<Product>? _products;

public async Task<IEnumerable<Product>> GetAllAsync()
{
    if (_products == null)
    {
        _products = LoadProductsFromJson();
    }
    return _products;
}
```

### 3. **Caching**
```csharp
// Cache dashboard data for 5 minutes
private DashboardSummary? _cachedSummary;
private DateTime _cacheTime;

public async Task<DashboardSummary?> GetSummaryAsync()
{
    if (_cachedSummary != null && 
        DateTime.Now - _cacheTime < TimeSpan.FromMinutes(5))
    {
        return _cachedSummary;
    }

    _cachedSummary = LoadSummaryFromJson();
    _cacheTime = DateTime.Now;
    return _cachedSummary;
}
```

---

## 🐛 Troubleshooting

### Issue: "JSON file not found"
**Solution:**
1. Check file path: `AppDomain.CurrentDomain.BaseDirectory`
2. Copy JSON files to output directory:
   ```xml
   <ItemGroup>
     <None Update="Mocks\Data\Json\*.json">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

### Issue: "JsonException: Invalid JSON"
**Solution:**
1. Validate JSON at https://jsonlint.com
2. Check encoding (UTF-8)
3. Check for trailing commas

### Issue: "GUID parse error"
**Solution:**
```csharp
// Use TryParse for safety
if (Guid.TryParse(item.GetProperty("id").GetString(), out var id))
{
    product.Id = id;
}
```

---

## 📚 References

- **JSON Files**: `src/MyShop.Plugins/Mocks/Data/Json/`
- **Entities**: `src/MyShop.Data/Entities/`
- **Interfaces**: `src/MyShop.Data/Repositories/Interfaces/`
- **DTOs**: `src/MyShop.Shared/DTOs/`

---

**📅 Created:** November 10, 2025  
**👤 Author:** AI Assistant  
**🎯 Status:** ✅ Phase 1 Complete - Ready for DI Setup!
