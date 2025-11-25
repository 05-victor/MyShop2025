using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentDashboardViewModel : BaseViewModel
{
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;
        private readonly IReportRepository _reportRepository;
        private readonly ICommissionRepository _commissionRepository;
        private readonly IOrderRepository _orderRepository;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _title = "Salesman Dashboard";

        [ObservableProperty]
        private bool _isVerified = true;

        [ObservableProperty]
        private bool _profileCompleted = true;

        // KPI Cards
        [ObservableProperty]
        private int _totalProducts = 0;

        [ObservableProperty]
        private int _totalSales = 0;

        [ObservableProperty]
        private decimal _totalCommission = 0m;

        [ObservableProperty]
        private decimal _totalRevenue = 0m;

        [ObservableProperty]
        private string _thisWeekCommission = "0%";

        // Top Performing Links
        [ObservableProperty]
        private ObservableCollection<TopAffiliateLink> _topLinks = new();

        // Recent Orders
        [ObservableProperty]
        private ObservableCollection<RecentSalesOrder> _recentOrders = new();

        public SalesAgentDashboardViewModel(
            INavigationService navigationService,
            IToastService toastHelper,
            ICredentialStorage credentialStorage,
            IReportRepository reportRepository,
            ICommissionRepository commissionRepository,
            IOrderRepository orderRepository)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            _credentialStorage = credentialStorage;
            _reportRepository = reportRepository;
            _commissionRepository = commissionRepository;
            _orderRepository = orderRepository;
        }

        public void Initialize(User user)
        {
            CurrentUser = user;
            IsVerified = user.IsEmailVerified;
            _ = LoadDashboardDataAsync(); // Fire and forget with exception handling
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[SalesmanDashboard] Starting LoadDashboardDataAsync...");
                
                // Get current user ID (use mock ID if not available)
                var userId = CurrentUser?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Loading data for user: {userId}");

                // Load performance metrics
                var metricsResult = await _reportRepository.GetPerformanceMetricsAsync(userId);
                if (metricsResult.IsSuccess && metricsResult.Data != null)
                {
                    TotalProducts = metricsResult.Data.TotalProductsShared;
                    TotalSales = metricsResult.Data.TotalOrders;
                    TotalCommission = metricsResult.Data.TotalCommission;
                    TotalRevenue = metricsResult.Data.TotalRevenue;
                }

                // Load commission summary for trend
                var commissionResult = await _commissionRepository.GetSummaryAsync(userId);
                if (commissionResult.IsSuccess && commissionResult.Data != null)
                {
                    var commissionSummary = commissionResult.Data;
                    // Calculate trend safely with division-by-zero protection
                    if (commissionSummary.LastMonthEarnings > 0)
                    {
                        var percentChange = ((commissionSummary.ThisMonthEarnings - commissionSummary.LastMonthEarnings) / commissionSummary.LastMonthEarnings * 100);
                        ThisWeekCommission = percentChange >= 0 
                            ? $"+{percentChange:F1}%" 
                            : $"{percentChange:F1}%";
                    }
                    else
                    {
                        ThisWeekCommission = commissionSummary.ThisMonthEarnings > 0 ? "+100%" : "0%";
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Commission trend: {ThisWeekCommission}");

                // Load top performing products
                await LoadTopLinksAsync(userId);

                // Load recent orders
                await LoadRecentOrdersAsync(userId);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] ❌ Error loading data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Stack trace: {ex.StackTrace}");
                TopLinks.Clear();
                RecentOrders.Clear();
                _toastHelper?.ShowError($"Failed to load dashboard: {ex.Message}");
            }
        }

        private async Task LoadTopLinksAsync(Guid userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Loading top products for user: {userId}");
                var topProductsResult = await _reportRepository.GetTopProductsAsync(userId, 3);

                TopLinks.Clear();
                if (topProductsResult.IsSuccess && topProductsResult.Data != null)
                {
                    foreach (var product in topProductsResult.Data)
                    {
                        TopLinks.Add(new TopAffiliateLink
                        {
                            Product = product.ProductName,
                            Clicks = product.Clicks,
                            Orders = product.TotalSold,
                            Revenue = product.TotalRevenue,
                            Commission = product.TotalCommission,
                            Status = "Active"
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] ❌ Error loading top links: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Stack trace: {ex.StackTrace}");
            }
        }

        private async Task LoadRecentOrdersAsync(Guid userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Loading recent orders for user: {userId}");
                var ordersResult = await _orderRepository.GetBySalesAgentIdAsync(userId);
                if (!ordersResult.IsSuccess || ordersResult.Data == null)
                {
                    return;
                }
                
                var recentOrders = ordersResult.Data.Take(5).ToList();

                RecentOrders.Clear();
                foreach (var order in recentOrders)
                {
                    var commissionResult = await _commissionRepository.GetByOrderIdAsync(order.Id);
                    var commission = commissionResult.IsSuccess ? commissionResult.Data : null;

                    RecentOrders.Add(new RecentSalesOrder
                    {
                        OrderId = $"ORD-{order.Id.ToString()[..8]}",
                        Customer = order.CustomerName,
                        Product = order.OrderItems.FirstOrDefault()?.ProductName ?? "Unknown",
                        OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                        Amount = order.FinalPrice,
                        Commission = commission?.CommissionAmount ?? 0m,
                        Status = commission?.Status ?? "Pending"
                    });
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] ❌ Error loading recent orders: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesmanDashboard] Stack trace: {ex.StackTrace}");
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await _credentialStorage.RemoveToken();
            await _toastHelper.ShowInfo("Logged out");
            await _navigationService.NavigateTo(typeof(LoginPage).FullName!);
        }
    }

    // Helper classes for data binding
    public class TopAffiliateLink
    {
        public string Product { get; set; } = string.Empty;
        public int Clicks { get; set; }
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RecentSalesOrder
    {
        public string OrderId { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public string Status { get; set; } = string.Empty;
}