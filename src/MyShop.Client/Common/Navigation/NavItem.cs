using System;

namespace MyShop.Client.Common.Navigation;

/// <summary>
/// Represents a navigation menu item with its metadata
/// </summary>
public sealed record NavItem(
    string Tag,
    string Title,
    Type PageType,
    string IconGlyph,
    params string[] Roles)
{
    /// <summary>
    /// Checks if this navigation item is accessible by the specified role
    /// </summary>
    public bool IsAccessibleBy(string role) => 
        Roles.Length == 0 || Array.Exists(Roles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
}
