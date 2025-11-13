using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.Dashboard;

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
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<DashboardShellViewModel>();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is User user)
            {
                ViewModel.Initialize(user);

                // Chỉ set mặc định lần đầu
                if (_isInitialized) return;
                _isInitialized = true;

                // 1. Mở nội dung Dashboard
                ContentFrame.Navigate(typeof(AdminDashboardPage), user);

                // 2. Chọn sẵn item Dashboard trong NavigationView
                _currentContentItem = FindNavigationItemByTag("dashboard");
                if (_currentContentItem != null)
                {
                    _isRestoringSelection = true;      // tránh loop SelectionChanged
                    Nav.SelectedItem = _currentContentItem;
                    _isRestoringSelection = false;
                }
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
                    ViewModel.NavigateToProductsCommand?.Execute(null);
                    RestoreSelection();
                    break;

                case "orders":
                    ViewModel.NavigateToOrdersCommand.Execute(null);
                    RestoreSelection();
                    break;

                case "reports":
                    ViewModel.NavigateToReportsCommand.Execute(null);
                    RestoreSelection();
                    break;

                case "settings":
                    ViewModel.NavigateToSettingsCommand.Execute(null);
                    RestoreSelection();
                    break;

                case "profile":
                    ViewModel.NavigateToProfileCommand.Execute(null);
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
