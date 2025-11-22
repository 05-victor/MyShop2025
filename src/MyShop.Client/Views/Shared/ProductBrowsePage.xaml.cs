using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.InitializeAsync();
        }
    }
}
