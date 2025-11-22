using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation cho Salesman role
/// </summary>
public class SalesmanDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.Salesman;

    public Type GetDashboardPageType()
    => typeof(MyShop.Client.Views.Shell.SalesAgentDashboardShell);

    public bool CanAccessFeature(string featureName)
    {
        // Salesman có quyền hạn chế
        var allowedFeatures = new[]
        {
            "Orders",
            "Products",
            "Customers"
        };

        return Array.Exists(allowedFeatures, f => 
            f.Equals(featureName, StringComparison.OrdinalIgnoreCase));
    }
}
