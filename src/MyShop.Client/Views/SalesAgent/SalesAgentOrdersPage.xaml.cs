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
    public sealed partial class SalesAgentOrdersPage : Page
    {
        public SalesAgentOrdersViewModel ViewModel { get; }

        public SalesAgentOrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentOrdersViewModel>();
            this.DataContext = ViewModel;
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
                LoggingService.Instance.Error("[SalesAgentOrdersPage] OnNavigatedTo failed", ex);
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
                var suggestions = ViewModel.Items
                    .Where(o => o.OrderId.ToLower().Contains(query) ||
                               o.CustomerName.ToLower().Contains(query) ||
                               o.ProductDescription.ToLower().Contains(query))
                    .Select(o => o.CustomerName)
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
            // Guard: ViewModel may be null during page initialization
            if (ViewModel == null) return;
            
            if (StatusComboBox.SelectedItem is ComboBoxItem item)
            {
                var status = item.Tag?.ToString() ?? "All";
                ViewModel.SelectedStatus = status;
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            
            if (SortComboBox.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag?.ToString() ?? "date-desc";
                var parts = tag.Split('-');
                if (parts.Length == 2)
                {
                    ViewModel.SortBy = parts[0];
                    ViewModel.SortDescending = parts[1] == "desc";
                }
            }
        }

        #endregion

        #region Pagination Event Handler

        private async void OnPageChanged(object sender, PageChangedEventArgs e)
        {
            if (ViewModel == null) return;

            ViewModel.CurrentPage = e.CurrentPage;
            ViewModel.PageSize = e.PageSize;
            
            await ViewModel.GoToPageAsync(e.CurrentPage);
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
                LoggingService.Instance.Error("Failed to refresh orders", ex);
            }
        }
    }
}
