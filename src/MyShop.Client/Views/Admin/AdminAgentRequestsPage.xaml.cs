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
    }
}
