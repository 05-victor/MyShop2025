using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;

namespace MyShop.Client.Extensions;

/// <summary>
/// Extension methods for ScrollViewer to help with scroll management
/// </summary>
public static class ScrollViewerExtensions
{
    /// <summary>
    /// Recursively find all ScrollViewers in a DependencyObject tree
    /// </summary>
    public static IEnumerable<ScrollViewer> FindScrollViewers(this DependencyObject parent)
    {
        if (parent == null)
            yield break;

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is ScrollViewer scrollViewer)
            {
                yield return scrollViewer;
            }

            // Recursively search children
            foreach (var nested in FindScrollViewers(child))
            {
                yield return nested;
            }
        }
    }

    /// <summary>
    /// Find the first ScrollViewer in a DependencyObject tree
    /// </summary>
    public static ScrollViewer? FindFirstScrollViewer(this DependencyObject parent)
    {
        if (parent == null)
            return null;

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            var nested = FindFirstScrollViewer(child);
            if (nested != null)
                return nested;
        }

        return null;
    }

    /// <summary>
    /// Scroll to top of the first ScrollViewer found
    /// </summary>
    public static void ScrollToTop(this Page page)
    {
        var scrollViewer = page.FindFirstScrollViewer();
        scrollViewer?.ChangeView(null, 0, null, disableAnimation: false);
    }

    /// <summary>
    /// Scroll all ScrollViewers to top
    /// </summary>
    public static void ScrollAllToTop(this Page page)
    {
        foreach (var scrollViewer in page.FindScrollViewers())
        {
            scrollViewer.ChangeView(null, 0, null, disableAnimation: false);
        }
    }
}
