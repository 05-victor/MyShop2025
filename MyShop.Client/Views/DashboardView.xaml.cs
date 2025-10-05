using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Views
{
    public sealed partial class DashboardView : Page
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<DashboardViewModel>();
            this.DataContext = ViewModel;
        }
    }
}