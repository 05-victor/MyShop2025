using MyShop.Client.Views.Shell;
using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation for Admin role.
/// </summary>
public class AdminDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.Admin;

    public Type GetDashboardPageType()
    => typeof(AdminDashboardShell);

    public bool CanAccessFeature(string featureName)
    {
        // Admin has access to all features
        return true;
    }
}
