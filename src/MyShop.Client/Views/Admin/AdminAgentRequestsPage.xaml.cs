using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.Admin
{
    public sealed partial class AdminAgentRequestsPage : Page
    {
        public AdminAgentRequestsViewModel ViewModel { get; }

        public AdminAgentRequestsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<AdminAgentRequestsViewModel>();
            this.DataContext = ViewModel;

            // Subscribe to pagination events for scroll-to-top
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Scroll to top when page changes
            if (e.PropertyName == nameof(ViewModel.CurrentPage))
            {
                PageScrollViewer?.ChangeView(null, 0, null, disableAnimation: false);
            }
        }

        #region Pagination Event Handlers

        /// <summary>
        /// Handle page change from PaginationControl
        /// </summary>
        private async void PaginationControl_PageChanged(object sender, int newPage)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsPage] PaginationControl_PageChanged - New page: {newPage}");
            try
            {
                // The CurrentPage property is already updated via TwoWay binding
                // Just trigger RefreshAsync to fetch new data
                await ViewModel.RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsPage] PaginationControl_PageChanged error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle page size change from PaginationControl
        /// </summary>
        private async void PaginationControl_PageSizeChanged(object sender, int newPageSize)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsPage] PaginationControl_PageSizeChanged - New page size: {newPageSize}");
            try
            {
                // Reset to page 1 when page size changes
                ViewModel.CurrentPage = 1;
                await ViewModel.RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminAgentRequestsPage] PaginationControl_PageSizeChanged error: {ex.Message}");
            }
        }

        #endregion
    }
}
