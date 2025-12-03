using System;
using System.Runtime.CompilerServices;

namespace MyShop.Client.Services;

/// <summary>
/// [DEPRECATED] Legacy logging wrapper - maintained for backward compatibility only.
/// 
/// ⚠️ NEW CODE SHOULD USE: MyShop.Client.Services.LoggingService.Instance
/// 
/// This is now a thin wrapper around LoggingService with Serilog infrastructure.
/// All methods forward to LoggingService for unified logging.
/// </summary>
[Obsolete("Use LoggingService.Instance instead. This wrapper exists only for backward compatibility.", false)]
public static class AppLogger
{
    public static void Info(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Information(message, caller, file);

    public static void Success(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Information(message, caller, file);

    public static void Warning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Warning(message, caller, file);

    public static void Error(string message, Exception? exception = null, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Error(message, exception, caller, file);

    public static void Debug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Debug(message, caller, file);

    public static void Enter([CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Debug($"→ {caller}", caller, file);

    public static void Exit([CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Debug($"← {caller}", caller, file);

    public static void Custom(string emoji, string category, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        => LoggingService.Instance.Information($"[{category}] {message}", caller, file);

    // Specialized logging - forward to LoggingService
    public static void Navigation(string fromPage, string toPage, object? parameter = null)
        => LoggingService.Instance.LogNavigation(fromPage, toPage, parameter, true);

    public static void Api(string operation, string endpoint, bool isSuccess, string? errorMessage = null)
        => LoggingService.Instance.LogApiCall(operation, endpoint, isSuccess, null, errorMessage);

    public static void Data(string operation, string entity, int? count = null)
        => LoggingService.Instance.LogDataOperation(operation, entity, count, true);

    public static void Auth(string action, string? username = null, bool isSuccess = true)
        => LoggingService.Instance.LogAuth(action, username, isSuccess);

    public static void Performance(string operation, long milliseconds)
        => LoggingService.Instance.LogPerformance(operation, milliseconds);

    public static void Memory(string context)
    {
        var memoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        LoggingService.Instance.Information($"Memory: {context} = {memoryMB:F2} MB");
    }

    public static void Separator(string? title = null)
        => LoggingService.Instance.Information(title != null ? $"═══ {title} ═══" : "═══════════════════════════════");

    // Utility methods - delegate to LoggingService
    public static string GetLogFilePath() => LoggingService.Instance.GetLogDirectory();
    public static string GetLogDirectory() => LoggingService.Instance.GetLogDirectory();
    public static bool IsFileLoggingEnabled() => LoggingService.Instance.IsInitialized;
}