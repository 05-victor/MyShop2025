using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.Dashboard;
using MyShop.Client.Views.Product;
using MyShop.Client.Views.Profile;
using MyShop.Client.Views.Settings;
using MyShop.Client.Helpers;

namespace MyShop.Client.Views.Shell
{
    public sealed partial class DashboardShellPage : Page
    {
        public DashboardShellViewModel ViewModel { get; }

        // Lưu item “nội dung hiện tại” (Dashboard,…)
        private NavigationViewItem? _currentContentItem;
        private bool _isRestoringSelection;
        private bool _isInitialized;

        public DashboardShellPage()
        {
            try
            {
                AppLogger.Enter();
                AppLogger.Info("Initializing DashboardShellPage...");
                InitializeComponent();
                AppLogger.Success("InitializeComponent completed");
                
                ViewModel = App.Current.Services.GetRequiredService<DashboardShellViewModel>();
                AppLogger.Debug("ViewModel resolved from DI");
                
                DataContext = ViewModel;
                AppLogger.Success("DashboardShellPage constructed successfully");
                AppLogger.Exit();
            }
            catch (Exception ex)
            {
                AppLogger.Error("DashboardShellPage constructor failed", ex);
                throw;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                AppLogger.Enter();
                base.OnNavigatedTo(e);

                if (e.Parameter is User user)
                {
                    AppLogger.Info($"User parameter received: {user.Username}");
                    ViewModel.Initialize(user);
                    AppLogger.Success("ViewModel initialized with user");

                    // Chỉ set mặc định lần đầu
                    if (_isInitialized)
                    {
                        AppLogger.Debug("Already initialized, skipping default setup");
                        return;
                    }
                    _isInitialized = true;

                    // 1. Mở nội dung Dashboard
                    AppLogger.Info("Navigating ContentFrame to AdminDashboardPage...");
                    ContentFrame.Navigate(typeof(AdminDashboardPage), user);
                    AppLogger.Success("ContentFrame navigation completed");

                    // 2. Chọn sẵn item Dashboard trong NavigationView
                    _currentContentItem = FindNavigationItemByTag("dashboard");
                    if (_currentContentItem != null)
                    {
                        AppLogger.Debug("Setting NavigationView selected item to dashboard");
                        _isRestoringSelection = true;      // tránh loop SelectionChanged
                        Nav.SelectedItem = _currentContentItem;
                        _isRestoringSelection = false;
                        AppLogger.Success("NavigationView selection set");
                    }
                }
                else
                {
                    AppLogger.Warning("No User parameter provided");
                }
                AppLogger.Exit();
            }
            catch (Exception ex)
            {
                AppLogger.Error("OnNavigatedTo failed", ex);
                throw;
            }
        }

        private NavigationViewItem? FindNavigationItemByTag(string tag)
        {
            return Nav.MenuItems
                      .OfType<NavigationViewItem>()
                      .FirstOrDefault(i => (i.Tag as string) == tag);
        }

        private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (_isRestoringSelection)
                return;

            if (args.SelectedItemContainer is not NavigationViewItem item)
                return;

            var tag = item.Tag as string ?? string.Empty;

            switch (tag)
            {
                case "dashboard":
                    _currentContentItem = item;   // ghi nhớ “tab nội dung” mới
                    if (ViewModel.CurrentUser != null)
                        ContentFrame.Navigate(typeof(AdminDashboardPage), ViewModel.CurrentUser);
                    else
                        ContentFrame.Navigate(typeof(AdminDashboardPage));
                    break;

                case "products":
                    _currentContentItem = item;
                    if (ViewModel.CurrentUser != null)
                        ContentFrame.Navigate(typeof(AdminProductPage), ViewModel.CurrentUser);
                    else
                        ContentFrame.Navigate(typeof(AdminProductPage));
                    break;

                case "profile":
                    _currentContentItem = item;
                    ContentFrame.Navigate(typeof(ProfilePage));
                    break;

                case "settings":
                    _currentContentItem = item;
                    ContentFrame.Navigate(typeof(SettingsPage));
                    break;

                case "orders":
                    ViewModel.NavigateToOrdersCommand.Execute(null);
                    RestoreSelection();
                    break;

                case "reports":
                    ViewModel.NavigateToReportsCommand.Execute(null);
                    RestoreSelection();
                    break;

                case "users":
                    ViewModel.NavigateToUsersCommand.Execute(null);
                    RestoreSelection();
                    break;

                case "agentRequests":
                    ViewModel.NavigateToAgentRequestsCommand.Execute(null);
                    RestoreSelection();
                    break;

            }
        }

        private void RestoreSelection()
        {
            if (_currentContentItem == null)
                return;

            _isRestoringSelection = true;
            Nav.SelectedItem = _currentContentItem;
            _isRestoringSelection = false;
        }
    }
}
