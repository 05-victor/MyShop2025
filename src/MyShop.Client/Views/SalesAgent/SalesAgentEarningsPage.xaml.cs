using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Client.Services;
using MyShop.Client.Views.Components.Pagination;
using System;
using System.Linq;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class EarningsPage : Page
    {
        public SalesAgentEarningsViewModel ViewModel { get; }

        public EarningsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentEarningsViewModel>();
            this.DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                if (ViewModel.InitializeCommand?.CanExecute(null) == true)
                {
                    await ViewModel.InitializeCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("[EarningsPage] OnNavigatedTo failed", ex);
            }
        }

        #region Search Card Event Handlers

        private void SearchCard_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text?.ToLower() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(query))
                {
                    sender.ItemsSource = null;
                    return;
                }

                // Generate suggestions from current items
                var suggestions = ViewModel.Commissions
                    .Where(c => c.OrderId.ToLower().Contains(query) ||
                               c.CustomerName.ToLower().Contains(query) ||
                               c.ProductName.ToLower().Contains(query))
                    .Select(c => c.CustomerName)
                    .Distinct()
                    .Take(8)
                    .ToList();

                sender.ItemsSource = suggestions;
            }
        }

        private void SearchCard_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem?.ToString() ?? string.Empty;
        }

        private async void SearchCard_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                ViewModel.SearchQuery = args.ChosenSuggestion.ToString() ?? string.Empty;
            }
            else
            {
                ViewModel.SearchQuery = args.QueryText;
            }

            if (ViewModel.ApplyFiltersCommand?.CanExecute(null) == true)
            {
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            }
        }

        #endregion

        #region Filter Event Handlers

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            
            if (StatusComboBox.SelectedItem is ComboBoxItem item)
            {
                ViewModel.SelectedStatus = item.Tag?.ToString() ?? "All";
            }
        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            
            if (PeriodComboBox.SelectedItem is ComboBoxItem item)
            {
                ViewModel.SelectedPeriod = item.Content?.ToString() ?? "All Time";
            }
        }

        #endregion

        #region Pagination Event Handler

        private async void OnPageChanged(object sender, PageChangedEventArgs e)
        {
            if (ViewModel == null) return;

            ViewModel.CurrentPage = e.CurrentPage;
            ViewModel.PageSize = e.PageSize;
            
            if (ViewModel.ApplyFiltersCommand?.CanExecute(null) == true)
            {
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            }
        }

        #endregion

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using var deferral = args.GetDeferral();
            try
            {
                if (ViewModel.RefreshCommand?.CanExecute(null) == true)
                {
                    await ViewModel.RefreshCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to refresh earnings", ex);
            }
        }

        private void ExportPdfButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Export PDF functionality - placeholder for now
            LoggingService.Instance.Information("Export PDF requested");
        }
    }
}
