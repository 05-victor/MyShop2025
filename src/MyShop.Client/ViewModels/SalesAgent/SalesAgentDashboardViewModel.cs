using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Facades;
using MyShop.Client.Services;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentDashboardViewModel : BaseViewModel
{
        private new readonly INavigationService _navigationService;
        private readonly IDashboardFacade _dashboardFacade;
        private readonly IProfileFacade _profileFacade;

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
            IDashboardFacade dashboardFacade,
            IProfileFacade profileFacade)
        {
            _navigationService = navigationService;
            _dashboardFacade = dashboardFacade;
            _profileFacade = profileFacade;
        }

        public void Initialize(User user)
        {
            try
            {
                LoggingService.Instance.LogViewModelEvent(
                    nameof(SalesAgentDashboardViewModel),
                    "Initialize",
                    $"User: {user.Username}, Roles: {string.Join(", ", user.Roles)}"
                );
                
                CurrentUser = user;
                IsVerified = user.IsEmailVerified;
                
                if (!user.IsEmailVerified)
                {
                    LoggingService.Instance.Warning($"User {user.Username} email not verified");
                }
                
                _ = LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error(
                    $"Failed to initialize {nameof(SalesAgentDashboardViewModel)}",
                    ex
                );
                GlobalExceptionHandler.LogException(ex, "SalesAgentDashboardViewModel.Initialize");
                throw;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                LoggingService.Instance.Information("Loading Sales Agent dashboard data...");
                SetLoadingState(true);
                
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await _dashboardFacade.LoadDashboardAsync("current");
                sw.Stop();
                
                LoggingService.Instance.LogPerformance(
                    "LoadDashboardAsync",
                    sw.ElapsedMilliseconds,
                    "SalesAgentDashboard"
                );
                
                if (!result.IsSuccess || result.Data == null)
                {
                    LoggingService.Instance.Warning($"Failed to load dashboard data: {result.ErrorMessage}");
                    SetLoadingState(false);
                    return;
                }

                var data = result.Data;
                TotalProducts = data.TotalProducts;
                TotalSales = data.TodayOrders;
                TotalCommission = Math.Round(data.MonthRevenue * 0.05m, 2);
                TotalRevenue = data.MonthRevenue;
                ThisWeekCommission = "+8.2%";

                TopLinks.Clear();
                if (data.TopSellingProducts != null)
                {
                    foreach (var product in data.TopSellingProducts.Take(3))
                    {
                        TopLinks.Add(new TopAffiliateLink
                        {
                            Product = product.Name ?? "Unknown",
                            Clicks = 0,
                            Orders = product.SoldCount,
                            Revenue = product.Revenue,
                            Commission = Math.Round(product.Revenue * 0.05m, 2),
                            Status = "Active"
                        });
                    }
                }

                RecentOrders.Clear();
                if (data.RecentOrders != null)
                {
                    foreach (var order in data.RecentOrders.Take(5))
                    {
                        RecentOrders.Add(new RecentSalesOrder
                        {
                            OrderId = $"ORD-{order.Id.ToString()[..8]}",
                            Customer = order.CustomerName ?? "Unknown",
                            Product = "Product",
                            OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                            Amount = order.TotalAmount,
                            Commission = Math.Round(order.TotalAmount * 0.05m, 2),
                            Status = order.Status ?? "Pending"
                        });
                    }
                }
                
                LoggingService.Instance.Information(
                    $"Dashboard data loaded: {TotalProducts} products, {TotalSales} sales, ${TotalRevenue} revenue"
                );
                LoggingService.Instance.LogDataOperation(
                    "Load",
                    "DashboardData",
                    TopLinks.Count + RecentOrders.Count,
                    true
                );
                
                SetLoadingState(false);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to load Sales Agent dashboard data", ex);
                GlobalExceptionHandler.LogException(ex, "SalesAgentDashboardViewModel.LoadDashboardDataAsync");
                
                SetLoadingState(false);
                TopLinks.Clear();
                RecentOrders.Clear();
                SetError("Failed to load dashboard data. Please try again.", ex);
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Note: LogoutAsync should be in IAuthFacade, not IProfileFacade
            // TODO: Inject IAuthFacade and use _authFacade.LogoutAsync()
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