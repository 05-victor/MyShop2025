using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.SalesAgent;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesOrdersPage : Page
    {
        public SalesAgentOrdersViewModel ViewModel { get; }

        public SalesOrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentOrdersViewModel>();
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
                System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersPage] OnNavigatedTo failed: {ex.Message}");
            }
        }

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
    }
}
