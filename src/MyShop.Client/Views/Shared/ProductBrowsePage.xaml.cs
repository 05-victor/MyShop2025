using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared
{
    public sealed partial class ProductBrowsePage : Page
    {
        public ProductBrowseViewModel ViewModel { get; }

        public ProductBrowsePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ProductBrowseViewModel>();
            SetupKeyboardShortcuts();
        }

        private void SetupKeyboardShortcuts()
        {
            // Ctrl+F: Focus search box
            var searchShortcut = new KeyboardAccelerator { Key = VirtualKey.F, Modifiers = VirtualKeyModifiers.Control };
            searchShortcut.Invoked += (s, e) => { e.Handled = true; };
            KeyboardAccelerators.Add(searchShortcut);

            // F5 or Ctrl+R: Refresh
            var refreshShortcut1 = new KeyboardAccelerator { Key = VirtualKey.F5 };
            refreshShortcut1.Invoked += async (s, e) => { await ViewModel.RefreshCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(refreshShortcut1);

            var refreshShortcut2 = new KeyboardAccelerator { Key = VirtualKey.R, Modifiers = VirtualKeyModifiers.Control };
            refreshShortcut2.Invoked += async (s, e) => { await ViewModel.RefreshCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(refreshShortcut2);

            // Ctrl+Down: Load more products
            var loadMoreShortcut = new KeyboardAccelerator { Key = VirtualKey.Down, Modifiers = VirtualKeyModifiers.Control };
            loadMoreShortcut.Invoked += async (s, e) => { if (ViewModel.HasMoreItems) await ViewModel.LoadMoreProductsCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(loadMoreShortcut);
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
                System.Diagnostics.Debug.WriteLine($"[ProductBrowsePage] OnNavigatedTo failed: {ex.Message}");
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard: Prevent NullReferenceException during page initialization
            if (ViewModel == null) return;
            
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var sortOption = selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(sortOption))
                {
                    await ViewModel.SortCommand.ExecuteAsync(sortOption);
                }
            }
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using var deferral = args.GetDeferral();
            await ViewModel.RefreshCommand.ExecuteAsync(null);
        }
    }
}
