using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Common.Helpers;

/// <summary>
/// Helper class to safely initialize Pages with comprehensive error handling.
/// Prevents silent crashes from XAML parse errors and DI resolution failures.
/// 
/// Usage in Page constructor:
/// <code>
/// public MyPage()
/// {
///     var (success, viewModel) = PageInitializationHelper.SafeInitialize&lt;MyViewModel&gt;(
///         this,
///         nameof(MyPage),
///         vm => { /* optional: additional initialization */ }
///     );
///     
///     if (!success) return;
///     ViewModel = viewModel;
/// }
/// </code>
/// </summary>
public static class PageInitializationHelper
{
    /// <summary>
    /// Safely initialize a page with XAML loading and ViewModel resolution
    /// Returns (success, viewModel) tuple
    /// </summary>
    public static (bool success, TViewModel? viewModel) SafeInitialize<TViewModel>(
        Page page,
        string pageName,
        Action<TViewModel>? onViewModelResolved = null,
        Action? beforeInitializeComponent = null,
        Action? afterInitializeComponent = null)
        where TViewModel : class
    {
        try
        {
            // Execute pre-initialization logic if provided
            beforeInitializeComponent?.Invoke();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[{pageName}] Pre-initialization failed", ex);
            // Continue anyway, this is optional
        }

        // Step 1: Try to load XAML (InitializeComponent)
        try
        {
            // Use reflection to call InitializeComponent on the page
            var initMethod = page.GetType().GetMethod("InitializeComponent");
            if (initMethod != null)
            {
                initMethod.Invoke(page, null);
            }
            else
            {
                Services.LoggingService.Instance.Warning($"[{pageName}] InitializeComponent method not found");
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[{pageName}] XAML load failed in InitializeComponent", ex);
            
            // Create minimal fallback UI
            page.Content = new TextBlock
            {
                Text = $"Failed to load {pageName}.\n\nError: {ex.Message}\n\nCheck logs at: {Services.LoggingService.Instance.GetLogDirectory()}",
                Margin = new Microsoft.UI.Xaml.Thickness(24),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };
            
            return (false, null);
        }

        try
        {
            // Execute post-initialization logic if provided
            afterInitializeComponent?.Invoke();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Warning($"[{pageName}] Post-initialization logic failed", ex.ToString());
            // Continue anyway
        }

        // Step 2: Try to resolve ViewModel from DI
        TViewModel? viewModel = null;
        try
        {
            viewModel = App.Current.Services.GetRequiredService<TViewModel>();
            Services.LoggingService.Instance.Debug($"[{pageName}] ViewModel {typeof(TViewModel).Name} resolved successfully");
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[{pageName}] Failed to resolve ViewModel {typeof(TViewModel).Name}", ex);
            
            // Show error in the page instead of crashing
            page.Content = new StackPanel
            {
                Padding = new Microsoft.UI.Xaml.Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"⚠️ Dependency Injection Error",
                        FontSize = 20,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.OrangeRed)
                    },
                    new TextBlock
                    {
                        Text = $"Failed to resolve ViewModel: {typeof(TViewModel).Name}",
                        FontSize = 14,
                        TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = $"Error: {ex.Message}",
                        FontSize = 12,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                        TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = $"Check Bootstrapper.cs for proper DI registration.",
                        FontSize = 12,
                        Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 0)
                    },
                    new TextBlock
                    {
                        Text = $"Logs: {Services.LoggingService.Instance.GetLogDirectory()}",
                        FontSize = 11,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                    }
                }
            };
            
            return (false, null);
        }

        // Step 3: Execute optional ViewModel initialization logic
        if (viewModel != null && onViewModelResolved != null)
        {
            try
            {
                onViewModelResolved(viewModel);
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[{pageName}] ViewModel initialization callback failed", ex);
                // Don't fail the whole page, just log
            }
        }

        return (true, viewModel);
    }

    /// <summary>
    /// Wrap an async void event handler with try-catch and logging
    /// Use this for button clicks, keyboard accelerators, etc.
    /// </summary>
    public static Func<object, TArgs, System.Threading.Tasks.Task> WrapAsyncEventHandler<TArgs>(
        string pageName,
        string handlerName,
        Func<object, TArgs, System.Threading.Tasks.Task> handler)
    {
        return async (sender, args) =>
        {
            try
            {
                await handler(sender, args);
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[{pageName}] {handlerName} failed", ex);
            }
        };
    }
}
