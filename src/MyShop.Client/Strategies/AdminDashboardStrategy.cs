using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy implementation cho Admin role
/// </summary>
public class AdminDashboardStrategy : IRoleStrategy
{
    public UserRole Role => UserRole.Admin;

    public Type GetDashboardPageType()
    {
        // Reference tới Client Views (vì Strategies vẫn cần biết Page types)
        return typeof(MyShop.Client.Views.Dashboard.AdminDashboardPage);
    }

    public bool CanAccessFeature(string featureName)
    {
        // Admin có quyền access tất cả features
        return true;
    }
}
