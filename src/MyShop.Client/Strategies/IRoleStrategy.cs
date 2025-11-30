using MyShop.Shared.Models.Enums;

namespace MyShop.Client.Strategies;

/// <summary>
/// Strategy interface for role-based behavior.
/// Each role will have its own implementation.
/// </summary>
public interface IRoleStrategy
{
    /// <summary>
    /// Gets the role that this strategy handles.
    /// </summary>
    UserRole Role { get; }

    /// <summary>
    /// Gets the page type for the dashboard of this role.
    /// </summary>
    /// <returns>The Type of the dashboard page.</returns>
    Type GetDashboardPageType();

    /// <summary>
    /// Checks if this role can access a specific feature.
    /// </summary>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <returns>True if the role can access the feature, false otherwise.</returns>
    bool CanAccessFeature(string featureName);
}
