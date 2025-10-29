using MyShop.Client.Models.Enums;
using System;

namespace MyShop.Client.Core.Strategies
{
    /// <summary>
    /// Strategy implementation cho Admin role
    /// </summary>
    public class AdminDashboardStrategy : IRoleStrategy
    {
        public UserRole Role => UserRole.Admin;

        public Type GetDashboardPageType()
        {
            return typeof(Views.Dashboard.DashboardPage);
        }

        public bool CanAccessFeature(string featureName)
        {
            // Admin có quyền access tất cả features
            return true;
        }
    }
}
