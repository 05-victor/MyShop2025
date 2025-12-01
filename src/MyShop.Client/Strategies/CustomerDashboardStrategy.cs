using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation for Customer role.
/// </summary>
public class CustomerDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.Customer;

    public Type GetDashboardPageType()
    => typeof(MyShop.Client.Views.Shell.CustomerDashboardShell);

    public bool CanAccessFeature(string featureName)
    {
        // Customer can only view their own orders
        var allowedFeatures = new[]
        {
            "MyOrders",
            "Profile"
        };

        return Array.Exists(allowedFeatures, f => 
            f.Equals(featureName, StringComparison.OrdinalIgnoreCase));
    }
}
