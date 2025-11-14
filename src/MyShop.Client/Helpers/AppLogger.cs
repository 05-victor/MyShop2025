using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MyShop.Client.Helpers;

/// <summary>
/// Centralized logging helper for debugging and diagnostics.
/// Provides structured logging with emojis and colors for better readability in Output window.
/// </summary>
public static class AppLogger
{
    private static readonly bool _isDebugMode = Debugger.IsAttached;

    #region Log Levels

    /// <summary>
    /// Log informational message (🔵 Blue)
    /// </summary>
    public static void Info(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"ℹ️ [INFO] [{fileName}.{caller}] {message}");
    }

    /// <summary>
    /// Log success message (🟢 Green)
    /// </summary>
    public static void Success(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"✅ [SUCCESS] [{fileName}.{caller}] {message}");
    }

    /// <summary>
    /// Log warning message (🟡 Yellow)
    /// </summary>
    public static void Warning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"⚠️ [WARNING] [{fileName}.{caller}] {message}");
    }

    /// <summary>
    /// Log error message (🔴 Red)
    /// </summary>
    public static void Error(string message, Exception? exception = null, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"❌ [ERROR] [{fileName}.{caller}] {message}");
        
        if (exception != null)
        {
            System.Diagnostics.Debug.WriteLine($"   Exception: {exception.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"   Message: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"   HRESULT: 0x{exception.HResult:X8}");
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                System.Diagnostics.Debug.WriteLine($"   Stack Trace:\n{exception.StackTrace}");
            }
            
            if (exception.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"   Inner Exception: {exception.InnerException.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"   Inner Message: {exception.InnerException.Message}");
            }
        }
    }

    /// <summary>
    /// Log debug message (🔍 Gray)
    /// </summary>
    public static void Debug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"🔍 [DEBUG] [{fileName}.{caller}] {message}");
    }

    #endregion

    #region Structured Logging

    /// <summary>
    /// Log method entry (useful for tracing flow)
    /// </summary>
    public static void Enter([CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"▶️ [ENTER] {fileName}.{caller}()");
    }

    /// <summary>
    /// Log method exit
    /// </summary>
    public static void Exit([CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"◀️ [EXIT] {fileName}.{caller}()");
    }

    /// <summary>
    /// Log with custom emoji/icon
    /// </summary>
    public static void Custom(string emoji, string category, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        if (!_isDebugMode) return;
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
        System.Diagnostics.Debug.WriteLine($"{emoji} [{category}] [{fileName}.{caller}] {message}");
    }

    #endregion

    #region Specialized Logging

    /// <summary>
    /// Log navigation events
    /// </summary>
    public static void Navigation(string fromPage, string toPage, object? parameter = null)
    {
        if (!_isDebugMode) return;
        var paramInfo = parameter != null ? $" (with param: {parameter.GetType().Name})" : "";
        System.Diagnostics.Debug.WriteLine($"🧭 [NAV] {fromPage} → {toPage}{paramInfo}");
    }

    /// <summary>
    /// Log API/Repository calls
    /// </summary>
    public static void Api(string operation, string endpoint, bool isSuccess, string? errorMessage = null)
    {
        if (!_isDebugMode) return;
        if (isSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"🌐 [API] {operation} → {endpoint} ✅ Success");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"🌐 [API] {operation} → {endpoint} ❌ Failed: {errorMessage}");
        }
    }

    /// <summary>
    /// Log data operations (CRUD)
    /// </summary>
    public static void Data(string operation, string entity, int? count = null)
    {
        if (!_isDebugMode) return;
        var countInfo = count.HasValue ? $" ({count} items)" : "";
        System.Diagnostics.Debug.WriteLine($"💾 [DATA] {operation} {entity}{countInfo}");
    }

    /// <summary>
    /// Log authentication/authorization events
    /// </summary>
    public static void Auth(string action, string? username = null, bool isSuccess = true)
    {
        if (!_isDebugMode) return;
        var userInfo = username != null ? $" (User: {username})" : "";
        var status = isSuccess ? "✅" : "❌";
        System.Diagnostics.Debug.WriteLine($"🔐 [AUTH] {action}{userInfo} {status}");
    }

    #endregion

    #region Diagnostics

    /// <summary>
    /// Log performance metric
    /// </summary>
    public static void Performance(string operation, long milliseconds)
    {
        if (!_isDebugMode) return;
        System.Diagnostics.Debug.WriteLine($"⏱️ [PERF] {operation} took {milliseconds}ms");
    }

    /// <summary>
    /// Log memory usage (for debugging leaks)
    /// </summary>
    public static void Memory(string context)
    {
        if (!_isDebugMode) return;
        var memoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        System.Diagnostics.Debug.WriteLine($"💾 [MEMORY] {context}: {memoryMB:F2} MB");
    }

    /// <summary>
    /// Print separator line for better readability
    /// </summary>
    public static void Separator(string? title = null)
    {
        if (!_isDebugMode) return;
        if (string.IsNullOrEmpty(title))
        {
            System.Diagnostics.Debug.WriteLine("════════════════════════════════════════════════════════");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"═══════════════ {title} ═══════════════");
        }
    }

    #endregion

    #region Future: File Logging (Currently Disabled)

    // TODO: Implement file logging for production diagnostics
    // Features to add:
    // - Log rotation (daily/size-based)
    // - Log levels filtering
    // - Async file writing
    // - Structured logging (JSON format)
    // - Log archiving/cleanup
    
    /*
    private static readonly string _logDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyShop2025",
        "Logs"
    );

    public static void WriteToFile(string message, string level)
    {
        try
        {
            Directory.CreateDirectory(_logDirectory);
            var logFile = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyy-MM-dd}.log");
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level}] {message}\n";
            File.AppendAllText(logFile, logEntry);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }
    */

    #endregion
}
