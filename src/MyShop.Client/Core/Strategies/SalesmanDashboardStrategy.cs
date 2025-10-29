using MyShop.Client.Models.Enums;
using System;

namespace MyShop.Client.Core.Strategies
{
    /// <summary>
    /// Strategy implementation cho Salesman role
    /// </summary>
    public class SalesmanDashboardStrategy : IRoleStrategy
    {
        public UserRole Role => UserRole.Salesman;

        public Type GetDashboardPageType()
        {
            return typeof(Views.Dashboard.SalesmanDashboardPage);
        }

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
}
