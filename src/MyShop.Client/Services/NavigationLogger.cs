using System;
using System.Diagnostics;

namespace MyShop.Client.Services;

/// <summary>
/// Specialized logger for navigation events
/// Wraps Frame.Navigate with comprehensive error tracking
/// 
/// This helps catch navigation errors that WinUI 3 Frame swallows silently
/// </summary>
public static class NavigationLogger
{
    /// <summary>
    /// Safe navigation wrapper that logs entry, success, and errors
    /// Returns true if navigation succeeded, false otherwise
    /// </summary>
    public static bool SafeNavigate(
        Microsoft.UI.Xaml.Controls.Frame frame,
        Type pageType,
        object? parameter = null,
        string? context = null)
    {
        var contextInfo = context != null ? $" [{context}]" : "";
        var paramInfo = parameter != null ? $" with parameter: {parameter.GetType().Name}" : "";
        
        try
        {
            LoggingService.Instance.Information(
                $"Attempting navigation{contextInfo} to {pageType.Name}{paramInfo}"
            );

            // Check if page type is valid
            if (!typeof(Microsoft.UI.Xaml.Controls.Page).IsAssignableFrom(pageType))
            {
                LoggingService.Instance.Error(
                    $"Navigation failed{contextInfo}: {pageType.Name} is not a valid Page type"
                );
                return false;
            }

            // Attempt navigation
            var sw = Stopwatch.StartNew();
            var result = frame.Navigate(pageType, parameter);
            sw.Stop();

            if (result)
            {
                LoggingService.Instance.LogNavigation(
                    frame.CurrentSourcePageType?.Name ?? "Unknown",
                    pageType.Name,
                    parameter,
                    isSuccess: true
                );
                
                LoggingService.Instance.LogPerformance(
                    $"Navigation to {pageType.Name}",
                    sw.ElapsedMilliseconds,
                    context
                );
            }
            else
            {
                LoggingService.Instance.LogNavigation(
                    frame.CurrentSourcePageType?.Name ?? "Unknown",
                    pageType.Name,
                    parameter,
                    isSuccess: false,
                    errorMessage: "Frame.Navigate returned false"
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogNavigation(
                frame.CurrentSourcePageType?.Name ?? "Unknown",
                pageType.Name,
                parameter,
                isSuccess: false,
                errorMessage: ex.Message
            );
            
            LoggingService.Instance.Error(
                $"Navigation exception{contextInfo} to {pageType.Name}",
                ex
            );
            
            return false;
        }
    }

    /// <summary>
    /// Log when a page's OnNavigatedTo is called
    /// Helps track navigation lifecycle
    /// </summary>
    public static void LogNavigatedTo(string pageName, object? parameter = null)
    {
        var paramInfo = parameter != null ? $" (Parameter: {parameter.GetType().Name})" : "";
        LoggingService.Instance.Debug($"OnNavigatedTo: {pageName}{paramInfo}");
    }

    /// <summary>
    /// Log when a page's OnNavigatedFrom is called
    /// </summary>
    public static void LogNavigatedFrom(string pageName)
    {
        LoggingService.Instance.Debug($"OnNavigatedFrom: {pageName}");
    }

    /// <summary>
    /// Log navigation failure with full context
    /// </summary>
    public static void LogNavigationFailure(
        string fromPage,
        string toPage,
        Exception exception,
        object? parameter = null)
    {
        LoggingService.Instance.Error(
            $"Navigation failed: {fromPage} → {toPage}",
            exception
        );

        // Additional structured logging
        LoggingService.Instance.LogNavigation(
            fromPage,
            toPage,
            parameter,
            isSuccess: false,
            errorMessage: exception.Message
        );
    }

    /// <summary>
    /// Log when ViewModel initialization fails during navigation
    /// </summary>
    public static void LogViewModelInitializationError(
        string viewModelName,
        Exception exception)
    {
        LoggingService.Instance.Error(
            $"ViewModel initialization failed: {viewModelName}",
            exception
        );

        LoggingService.Instance.LogViewModelEvent(
            viewModelName,
            "InitializationFailed",
            exception.Message
        );
    }
}
