using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;

namespace MyShop.Client.Common.Navigation;

/// <summary>
/// Helper for creating NavigationView items from NavRegistry
/// </summary>
public static class NavigationViewHelper
{
    /// <summary>
    /// Create NavigationViewItems for a specific role from the registry
    /// </summary>
    public static IEnumerable<NavigationViewItem> CreateMenuItemsForRole(string role, string foregroundResourceKey)
    {
        var items = NavRegistry.GetItemsForRole(role);
        
        foreach (var navItem in items)
        {
            var item = new NavigationViewItem
            {
                Content = navItem.Title,
                Tag = navItem.Tag
            };

            // Set icon
            var icon = new FontIcon
            {
                Glyph = navItem.IconGlyph
            };

            // Apply foreground color if resource key provided
            if (!string.IsNullOrEmpty(foregroundResourceKey) && Application.Current.Resources.TryGetValue(foregroundResourceKey, out var resource))
            {
                if (resource is Brush brush)
                {
                    item.Foreground = brush;
                    icon.Foreground = brush;
                }
            }

            item.Icon = icon;

            yield return item;
        }
    }

    /// <summary>
    /// Find NavigationViewItem by tag
    /// </summary>
    public static NavigationViewItem? FindItemByTag(NavigationView navigationView, string tag)
    {
        return navigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag as string == tag);
    }
}
