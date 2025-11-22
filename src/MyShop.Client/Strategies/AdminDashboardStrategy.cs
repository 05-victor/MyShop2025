using MyShop.Client.Views.Shell;
using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation cho Admin role
/// </summary>
public class AdminDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.Admin;

    public Type GetDashboardPageType()
    => typeof(AdminDashboardShell);

    public bool CanAccessFeature(string featureName)
    {
        // Admin có quyền access tất cả features
        return true;
    }
}
