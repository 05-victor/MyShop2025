using MyShop.Client.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MyShop.Client.Helpers;

/// <summary>
/// Centralized logging helper for debugging and diagnostics.
/// Provides structured logging with emojis and colors for better readability in Output window.
/// Also writes to file for production diagnostics.
/// 
/// File logging có thể tắt/bật qua AppConfig.EnableLogging
/// Mỗi session tạo file riêng với timestamp: app_2025-11-15_14-30-45.log
/// </summary>
public static class AppLogger
{
    private static readonly bool _isDebugMode = Debugger.IsAttached;
    private static readonly string _logDirectory;
    private static readonly string _currentLogFile;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);
    private static readonly DateTime _sessionStartTime = DateTime.Now;

    static AppLogger()
    {
        // Lấy đường dẫn đến thư mục gốc của project (nơi có Helpers/)
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var binFolder = Path.GetDirectoryName(assemblyLocation);
        
        // Từ bin/x64/Debug/... đi lên đến project root
        var projectRoot = Directory.GetParent(binFolder!)?.Parent?.Parent?.Parent?.FullName;
        
        if (projectRoot != null)
        {
            _logDirectory = Path.Combine(projectRoot, "Logs");
        }
        else
        {
            // Fallback to AppData nếu không tìm thấy project root
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MyShop2025",
                "Logs"
            );
        }

        // Tạo tên file với timestamp của session (mỗi lần chạy app = 1 file mới)
        _currentLogFile = Path.Combine(
            _logDirectory, 
            $"app_{_sessionStartTime:yyyy-MM-dd_HH-mm-ss}.log"
        );

        // Tạo thư mục nếu chưa có
        try
        {
            Directory.CreateDirectory(_logDirectory);
            
            // Ghi log đầu tiên
            var sessionHeader = new string('=', 60) + "\n" +
                               $"SESSION START: {_sessionStartTime:yyyy-MM-dd HH:mm:ss}\n" +
                               $"App: MyShop 2025 WinUI Client\n" +
                               $"Log File: {_currentLogFile}\n" +
                               new string('=', 60) + "\n";
            File.WriteAllText(_currentLogFile, sessionHeader);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize logging: {ex.Message}");
        }
    }

    #region Log Levels

    /// <summary>
    /// Log informational message (🔵 Blue)
    /// </summary>
    public static void Info(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"ℹ️ [INFO] [{fileName}.{caller}] {message}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "INFO");
    }

    /// <summary>
    /// Log success message (🟢 Green)
    /// </summary>
    public static void Success(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"✅ [SUCCESS] [{fileName}.{caller}] {message}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "SUCCESS");
    }

    /// <summary>
    /// Log warning message (🟡 Yellow)
    /// </summary>
    public static void Warning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"⚠️ [WARNING] [{fileName}.{caller}] {message}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "WARNING");
    }

    /// <summary>
    /// Log error message (🔴 Red)
    /// </summary>
    public static void Error(string message, Exception? exception = null, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"❌ [ERROR] [{fileName}.{caller}] {message}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "ERROR");
        
        if (exception != null)
        {
            var exceptionDetails = $"   Exception: {exception.GetType().Name}\n" +
                                  $"   Message: {exception.Message}\n" +
                                  $"   HRESULT: 0x{exception.HResult:X8}";
            
            if (_isDebugMode)
            {
                System.Diagnostics.Debug.WriteLine(exceptionDetails);
            }
            
            WriteToFileAsync(exceptionDetails, "ERROR");
            
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                var stackTrace = $"   Stack Trace:\n{exception.StackTrace}";
                if (_isDebugMode)
                {
                    System.Diagnostics.Debug.WriteLine(stackTrace);
                }
                WriteToFileAsync(stackTrace, "ERROR");
            }
            
            if (exception.InnerException != null)
            {
                var innerException = $"   Inner Exception: {exception.InnerException.GetType().Name}\n" +
                                    $"   Inner Message: {exception.InnerException.Message}";
                if (_isDebugMode)
                {
                    System.Diagnostics.Debug.WriteLine(innerException);
                }
                WriteToFileAsync(innerException, "ERROR");
            }
        }
    }

    /// <summary>
    /// Log debug message (🔍 Gray)
    /// </summary>
    public static void Debug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"🔍 [DEBUG] [{fileName}.{caller}] {message}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "DEBUG");
    }

    #endregion

    #region Structured Logging

    /// <summary>
    /// Log method entry (useful for tracing flow)
    /// </summary>
    public static void Enter([CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"▶️ [ENTER] {fileName}.{caller}()";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "ENTER");
    }

    /// <summary>
    /// Log method exit
    /// </summary>
    public static void Exit([CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"◀️ [EXIT] {fileName}.{caller}()";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "EXIT");
    }

    /// <summary>
    /// Log with custom emoji/icon
    /// </summary>
    public static void Custom(string emoji, string category, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        var logMessage = $"{emoji} [{category}] [{fileName}.{caller}] {message}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, category);
    }

    #endregion

    #region Specialized Logging

    /// <summary>
    /// Log navigation events
    /// </summary>
    public static void Navigation(string fromPage, string toPage, object? parameter = null)
    {
        var paramInfo = parameter != null ? $" (with param: {parameter.GetType().Name})" : "";
        var logMessage = $"🧭 [NAV] {fromPage} → {toPage}{paramInfo}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "NAV");
    }

    /// <summary>
    /// Log API/Repository calls
    /// </summary>
    public static void Api(string operation, string endpoint, bool isSuccess, string? errorMessage = null)
    {
        string logMessage;
        if (isSuccess)
        {
            logMessage = $"🌐 [API] {operation} → {endpoint} ✅ Success";
        }
        else
        {
            logMessage = $"🌐 [API] {operation} → {endpoint} ❌ Failed: {errorMessage}";
        }
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "API");
    }

    /// <summary>
    /// Log data operations (CRUD)
    /// </summary>
    public static void Data(string operation, string entity, int? count = null)
    {
        var countInfo = count.HasValue ? $" ({count} items)" : "";
        var logMessage = $"💾 [DATA] {operation} {entity}{countInfo}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "DATA");
    }

    /// <summary>
    /// Log authentication/authorization events
    /// </summary>
    public static void Auth(string action, string? username = null, bool isSuccess = true)
    {
        var userInfo = username != null ? $" (User: {username})" : "";
        var status = isSuccess ? "✅" : "❌";
        var logMessage = $"🔐 [AUTH] {action}{userInfo} {status}";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "AUTH");
    }

    #endregion

    #region Diagnostics

    /// <summary>
    /// Log performance metric
    /// </summary>
    public static void Performance(string operation, long milliseconds)
    {
        var logMessage = $"⏱️ [PERF] {operation} took {milliseconds}ms";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "PERF");
    }

    /// <summary>
    /// Log memory usage (for debugging leaks)
    /// </summary>
    public static void Memory(string context)
    {
        var memoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        var logMessage = $"💾 [MEMORY] {context}: {memoryMB:F2} MB";
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "MEMORY");
    }

    /// <summary>
    /// Print separator line for better readability
    /// </summary>
    public static void Separator(string? title = null)
    {
        string logMessage;
        if (string.IsNullOrEmpty(title))
        {
            logMessage = "════════════════════════════════════════════════════════";
        }
        else
        {
            logMessage = $"═══════════════ {title} ═══════════════";
        }
        
        if (_isDebugMode)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
        
        WriteToFileAsync(logMessage, "SEP");
    }

    #endregion

    #region File Logging

    /// <summary>
    /// Async write to log file (nếu AppConfig.EnableLogging = true)
    /// Ghi vào file session hiện tại: app_YYYY-MM-DD_HH-mm-ss.log
    /// </summary>
    private static async void WriteToFileAsync(string message, string level)
    {
        // Kiểm tra config trước khi ghi file
        if (!AppConfig.Instance.EnableLogging) return;

        try
        {
            await _fileLock.WaitAsync();
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level}] {message}\n";
            
            await File.AppendAllTextAsync(_currentLogFile, logEntry);
        }
        catch (Exception ex)
        {
            // Fail silently, don't crash the app
            System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Cleanup log files older than 7 days
    /// <summary>
    /// Cleanup log files older than 30 days
    /// </summary>
    public static void CleanupOldLogs(int keepDays = 30)
    {
        try
        {
            var directory = new DirectoryInfo(_logDirectory);
            if (!directory.Exists) return;
            
            var cutoffDate = DateTime.Now.AddDays(-keepDays);
            
            var oldFiles = directory.GetFiles("app_*.log")
                .Where(f => f.CreationTime < cutoffDate);
            
            var deletedCount = 0;
            foreach (var file in oldFiles)
            {
                try
                {
                    file.Delete();
                    deletedCount++;
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            
            if (deletedCount > 0)
            {
                Info($"Cleaned up {deletedCount} old log files (older than {keepDays} days)");
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Get the current session log file path
    /// </summary>
    public static string GetLogFilePath()
    {
        return _currentLogFile;
    }

    /// <summary>
    /// Get the log directory path
    /// </summary>
    public static string GetLogDirectory()
    {
        return _logDirectory;
    }
    
    /// <summary>
    /// Get session start time
    /// </summary>
    public static DateTime GetSessionStartTime()
    {
        return _sessionStartTime;
    }
    
    /// <summary>
    /// Check if file logging is enabled
    /// </summary>
    public static bool IsFileLoggingEnabled()
    {
        return AppConfig.Instance.EnableLogging;
    }

    #endregion
}