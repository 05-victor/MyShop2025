using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using System;

namespace MyShop.Client.Views.Shared
{
    public sealed partial class PurchaseOrdersPage : Page
    {
        public PurchaseOrdersViewModel ViewModel { get; }

        public PurchaseOrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<PurchaseOrdersViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersPage] OnNavigatedTo failed: {ex.Message}");
            }
        }

        #region Filter Event Handlers

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (StatusComboBox.SelectedItem is ComboBoxItem item)
            {
                var status = item.Tag?.ToString() ?? "All";
                ViewModel.SelectedStatus = status;
            }
        }

        private void PaymentStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (PaymentStatusComboBox.SelectedItem is ComboBoxItem item)
            {
                var paymentStatus = item.Tag?.ToString() ?? "All";
                ViewModel.SelectedPaymentStatus = paymentStatus;
            }
        }

        #endregion

        private async void OnPageChanged(object sender, int currentPage)
        {
            ViewModel.CurrentPage = currentPage;
            await ViewModel.LoadDataAsync();
        }
    }
}