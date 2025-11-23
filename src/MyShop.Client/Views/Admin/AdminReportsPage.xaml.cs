using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.Admin;

/// <summary>
/// Reports page for Admin role
/// </summary>
public sealed partial class AdminReportsPage : Page
{
    public ReportsPageViewModel ViewModel { get; }

    public AdminReportsPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ReportsPageViewModel>();
        InitializeComponent();
    }
}
