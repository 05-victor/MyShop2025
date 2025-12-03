using System;
using System.Threading.Tasks;

namespace MyShop.Client.Services;

/// <summary>
/// Global exception handlers for WinUI 3 application
/// Catches all unhandled exceptions from multiple sources:
/// - UI Thread exceptions
/// - Task exceptions
/// - AppDomain unhandled exceptions
/// 
/// This ensures no exception goes unlogged
/// </summary>
public static class GlobalExceptionHandler
{
    private static bool _isInitialized;

    /// <summary>
    /// Initialize global exception handlers
    /// Must be called BEFORE any other app initialization
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized)
        {
            LoggingService.Instance.Warning("GlobalExceptionHandler already initialized");
            return;
        }

        // 1. AppDomain Unhandled Exceptions (synchronous)
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;

        // 2. Task Unobserved Exceptions (async without await)
        TaskScheduler.UnobservedTaskException += OnTaskUnobservedException;

        // 3. Current SynchronizationContext Unhandled Exceptions (UI thread)
        // Note: This is handled by App.UnhandledException in App.xaml.cs for WinUI 3

        _isInitialized = true;
        LoggingService.Instance.Information("Global exception handlers initialized");
    }

    /// <summary>
    /// Handles truly unhandled exceptions from AppDomain
    /// These are usually fatal and will crash the app
    /// </summary>
    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        var isFatal = e.IsTerminating;

        if (exception != null)
        {
            LoggingService.Instance.Fatal(
                $"AppDomain Unhandled Exception (Fatal: {isFatal})",
                exception
            );

            // Try to log additional context
            try
            {
                LoggingService.Instance.Information($"Exception Type: {exception.GetType().FullName}");
                LoggingService.Instance.Information($"Source: {exception.Source}");
                LoggingService.Instance.Information($"HResult: 0x{exception.HResult:X8}");
                
                if (exception.InnerException != null)
                {
                    LoggingService.Instance.Information(
                        $"Inner Exception: {exception.InnerException.GetType().FullName} - {exception.InnerException.Message}"
                    );
                }
            }
            catch
            {
                // If logging fails, at least we tried
            }
        }
        else
        {
            LoggingService.Instance.Fatal("AppDomain Unhandled Exception (non-exception object)");
        }

        // Force flush logs before potential crash
        Serilog.Log.CloseAndFlush();
    }

    /// <summary>
    /// Handles unobserved Task exceptions
    /// These happen when an async Task fails but nobody awaited or observed it
    /// </summary>
    private static void OnTaskUnobservedException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LoggingService.Instance.Error(
            "Task Unobserved Exception (async/await not properly handled)",
            e.Exception
        );

        // Log each inner exception in the AggregateException
        if (e.Exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                LoggingService.Instance.Error(
                    $"  - {innerException.GetType().Name}: {innerException.Message}",
                    innerException
                );
            }
        }

        // Mark as observed to prevent app crash
        e.SetObserved();
    }

    /// <summary>
    /// Manually log an exception with context
    /// Use this in catch blocks where you want to log but not necessarily throw
    /// </summary>
    public static void LogException(
        Exception exception,
        string context,
        bool isFatal = false)
    {
        if (isFatal)
        {
            LoggingService.Instance.Fatal($"Exception in {context}", exception);
        }
        else
        {
            LoggingService.Instance.Error($"Exception in {context}", exception);
        }
    }

    /// <summary>
    /// Log exception with additional structured data
    /// </summary>
    public static void LogExceptionWithData(
        Exception exception,
        string context,
        object? additionalData = null)
    {
        LoggingService.Instance.Error($"Exception in {context}", exception);

        if (additionalData != null)
        {
            try
            {
                var dataType = additionalData.GetType();
                LoggingService.Instance.Information($"Additional Data ({dataType.Name}): {additionalData}");
            }
            catch
            {
                // Ignore serialization errors
            }
        }
    }

    /// <summary>
    /// Check if exception handler is initialized
    /// </summary>
    public static bool IsInitialized => _isInitialized;
}
