using MyShop.Client.Core.Strategies;
using MyShop.Client.Models.Enums;

namespace MyShop.Client.Core.Services.Interfaces
{
    /// <summary>
    /// Factory để lấy strategy phù hợp với role
    /// </summary>
    public interface IRoleStrategyFactory
    {
        /// <summary>
        /// Lấy strategy tương ứng với role
        /// </summary>
        IRoleStrategy GetStrategy(UserRole role);
    }
}
