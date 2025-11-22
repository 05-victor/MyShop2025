using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.SalesAgent;

/// <summary>
/// Reports page for SalesAgent role
/// </summary>
public sealed partial class SalesAgentReportsPage : Page
{
    public ReportsPageViewModel ViewModel { get; }

    public SalesAgentReportsPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ReportsPageViewModel>();
        InitializeComponent();
    }
}
