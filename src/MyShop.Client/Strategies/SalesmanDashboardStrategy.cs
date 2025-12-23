using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation for SalesAgent role.
/// </summary>
public class SalesAgentDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.SalesAgent;

    public Type GetDashboardPageType()
    => typeof(MyShop.Client.Views.Shell.SalesAgentDashboardShell);

    public bool CanAccessFeature(string featureName)
    {
        // SalesAgent has limited access
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
