using MyShop.Client.Models.Enums;
using System;

namespace MyShop.Client.Core.Strategies
{
    /// <summary>
    /// Strategy interface cho role-based behavior
    /// Mỗi role sẽ có implementation riêng
    /// </summary>
    public interface IRoleStrategy
    {
        /// <summary>
        /// Role mà strategy này handle
        /// </summary>
        UserRole Role { get; }

        /// <summary>
        /// Lấy page type cho dashboard của role này
        /// </summary>
        Type GetDashboardPageType();

        /// <summary>
        /// Check xem role này có thể access feature không
        /// </summary>
        bool CanAccessFeature(string featureName);
    }
}
