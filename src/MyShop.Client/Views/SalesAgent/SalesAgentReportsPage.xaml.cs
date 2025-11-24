using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.SalesAgent;

/// <summary>
/// Reports page for SalesAgent role
/// </summary>
public sealed partial class SalesAgentReportsPage : Page
{
    public AdminReportsViewModel ViewModel { get; }

    public SalesAgentReportsPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<AdminReportsViewModel>();
        InitializeComponent();
    }
}
