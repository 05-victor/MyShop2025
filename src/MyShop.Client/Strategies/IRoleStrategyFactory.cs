using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

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
