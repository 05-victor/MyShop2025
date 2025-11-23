using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.SalesAgent;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class EarningsPage : Page
    {
        public EarningsViewModel ViewModel { get; }

        public EarningsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<EarningsViewModel>();
        }
    }
}
