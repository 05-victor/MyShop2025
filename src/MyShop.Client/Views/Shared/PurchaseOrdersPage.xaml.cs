using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using MyShop.Client.Views.Components.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace MyShop.Client.Views.Shared
{
    public sealed partial class PurchaseOrdersPage : Page
    {
        public PurchaseOrdersViewModel ViewModel { get; }
        
        private ToggleButton[] _filterButtons = null!;
        private bool _initialized = false;
        
        // Colors for toggle button states
        private static readonly SolidColorBrush SelectedBackground = new(Color.FromArgb(255, 37, 99, 235)); // #2563EB
        private static readonly SolidColorBrush SelectedForeground = new(Microsoft.UI.Colors.White);
        private static readonly SolidColorBrush NormalBackground = new(Microsoft.UI.Colors.White);
        private static readonly SolidColorBrush NormalForeground = new(Color.FromArgb(255, 107, 114, 128)); // #6B7280

        public PurchaseOrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<PurchaseOrdersViewModel>();
            
            // Set Tag for binding from DataTemplate
            OrdersRepeater.Tag = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                // Initialize filter buttons array after page is loaded
                _filterButtons = new[] { FilterAll, FilterPending, FilterPaid, FilterShipped, FilterDelivered };
                
                // Set initial button styling
                UpdateFilterButtonStyles();
                
                await ViewModel.InitializeAsync();
                _initialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersPage] OnNavigatedTo failed: {ex.Message}");
            }
        }

        #region Search Event Handlers

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text?.ToLower() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    sender.ItemsSource = null;
                    return;
                }

                // Generate suggestions from actual product names in orders (not ProductSummary)
                var suggestions = new List<string>();
                
                // Add matching Order IDs
                suggestions.AddRange(
                    ViewModel.Items
                        .Where(o => o.OrderId.ToLower().Contains(query))
                        .Select(o => o.OrderId)
                );
                
                // Add matching tracking numbers
                suggestions.AddRange(
                    ViewModel.Items
                        .Where(o => o.TrackingNumber.ToLower().Contains(query))
                        .Select(o => o.TrackingNumber)
                );
                
                // Add matching product names from OrderItems (the actual product names, not summary)
                suggestions.AddRange(
                    ViewModel.Items
                        .SelectMany(o => o.Items)
                        .Where(item => item.ProductName.ToLower().Contains(query))
                        .Select(item => item.ProductName)
                );

                sender.ItemsSource = suggestions.Distinct().Take(8).ToList();
            }
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                ViewModel.SearchQuery = args.ChosenSuggestion.ToString() ?? string.Empty;
            }
            else
            {
                ViewModel.SearchQuery = args.QueryText;
            }

            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }

        #endregion

        #region Filter & Sort Event Handlers

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedButton)
            {
                // Uncheck all other buttons
                foreach (var btn in _filterButtons)
                {
                    btn.IsChecked = (btn == clickedButton);
                }
                
                // Ensure clicked button stays checked
                clickedButton.IsChecked = true;
                
                // Update visual styles immediately
                UpdateFilterButtonStyles();
                
                // Get status from Tag
                var status = clickedButton.Tag?.ToString() ?? "All";
                ViewModel.SelectedStatus = status;
                
                // ViewModel.OnSelectedStatusChanged will automatically trigger LoadPageAsync
            }
        }
        
        private void UpdateFilterButtonStyles()
        {
            if (_filterButtons == null) return;
            
            foreach (var btn in _filterButtons)
            {
                if (btn.IsChecked == true)
                {
                    btn.Background = SelectedBackground;
                    btn.Foreground = SelectedForeground;
                    btn.BorderBrush = SelectedBackground;
                }
                else
                {
                    btn.Background = NormalBackground;
                    btn.Foreground = NormalForeground;
                    btn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 229, 231, 235)); // #E5E7EB
                }
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard against null ViewModel or early trigger
            if (ViewModel == null || !_initialized) return;
            
            if (SortComboBox.SelectedItem is ComboBoxItem item && item.Content is string sortOption)
            {
                System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersPage] Sort changed to: {sortOption}");
                
                // Set the value and force reload
                ViewModel.SelectedSort = sortOption;
                await ViewModel.LoadDataAsync();
            }
        }

        private async void OnPageChanged(object sender, PageChangedEventArgs e)
        {
            ViewModel.CurrentPage = e.CurrentPage;
            await ViewModel.LoadDataAsync();
        }

        #endregion
    }
}