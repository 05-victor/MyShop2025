using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Factory to get the appropriate strategy for a role.
/// </summary>
public interface IRoleStrategyFactory
{
    /// <summary>
    /// Gets the strategy corresponding to the specified role.
    /// </summary>
    /// <param name="role">The user role to get strategy for.</param>
    /// <returns>The role strategy instance.</returns>
    IRoleStrategy GetStrategy(UserRole role);
}
