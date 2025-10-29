using MyShop.Client.Models.Enums;
using System;

namespace MyShop.Client.Core.Strategies
{
    /// <summary>
    /// Strategy implementation cho Customer role
    /// </summary>
    public class CustomerDashboardStrategy : IRoleStrategy
    {
        public UserRole Role => UserRole.Customer;

        public Type GetDashboardPageType()
        {
            return typeof(Views.Dashboard.CustomerDashboardPage);
        }

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
}
