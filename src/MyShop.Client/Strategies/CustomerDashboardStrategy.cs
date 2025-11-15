using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation cho Customer role
/// </summary>
public class CustomerDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.Customer;

    public Type GetDashboardPageType()
    {
        return typeof(MyShop.Client.Views.Dashboard.CustomerDashboardPage);
    }

    //public Type GetDashboardPageType() => typeof(CustomerDashboardShellPage);

    public bool CanAccessFeature(string featureName)
    {
        // Customer chỉ có quyền xem orders của mình
        var allowedFeatures = new[]
        {
            "MyOrders",
            "Profile"
        };

        return Array.Exists(allowedFeatures, f => 
            f.Equals(featureName, StringComparison.OrdinalIgnoreCase));
    }
}
