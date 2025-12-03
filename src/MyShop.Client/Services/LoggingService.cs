using MyShop.Core.Common;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MyShop.Client.Services;

/// <summary>
/// Professional logging service using Serilog
/// Replaces the basic AppLogger with structured logging to file
/// 
/// Features:
/// - Rolling file per day (app-YYYY-MM-DD.log)
/// - Separate error file (errors-YYYY-MM-DD.log)
/// - Debug output for development
/// - Structured logging with properties
/// - Automatic cleanup of old logs (30 days retention)
/// 
/// Log Location:
/// - Development (StoreLogsInProject=true): MyShop.Client/Logs/
/// - Production (StoreLogsInProject=false): AppData/Local/MyShop2025/logs/
/// 
/// Toggle in ApiConfig.json: "StoreLogsInProject": true/false
/// </summary>
public sealed class LoggingService : IDisposable
{
    private static LoggingService? _instance;
    private static readonly object _lock = new();
    
    public static LoggingService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new LoggingService();
                }
            }
            return _instance;
        }
    }

    private readonly string _logDirectory;
    private bool _isInitialized;

    private LoggingService()
    {
        // Determine log directory based on configuration
        _logDirectory = DetermineLogDirectory();
        Directory.CreateDirectory(_logDirectory);
    }

    /// <summary>
    /// Determine the log directory based on settings
    /// 
    /// Priority:
    /// 1. If StoreLogsInProject=true AND we can find project root → Project/Logs
    /// 2. Otherwise → AppData/Local/MyShop2025/logs
    /// </summary>
    private string DetermineLogDirectory()
    {
        // Check if we should store logs in project (development mode)
        var storeInProject = Config.AppConfig.Instance.StoreLogsInProject;
        
        if (storeInProject)
        {
            // Try to find project root from assembly location
            var projectLogDir = TryGetProjectLogDirectory();
            
            if (!string.IsNullOrEmpty(projectLogDir))
            {
                // Update StorageConstants with the project log directory
                StorageConstants.LogSettings.ProjectLogDirectory = projectLogDir;
                StorageConstants.LogSettings.StoreInProjectDirectory = true;
                
                System.Diagnostics.Debug.WriteLine($"[LoggingService] Using PROJECT log directory: {projectLogDir}");
                return projectLogDir;
            }
            
            System.Diagnostics.Debug.WriteLine("[LoggingService] Could not determine project root, falling back to AppData");
        }

        // Use AppData location (production mode or fallback)
        StorageConstants.LogSettings.StoreInProjectDirectory = false;
        StorageConstants.EnsureDirectoryExists(StorageConstants.LogsDirectory);
        
        System.Diagnostics.Debug.WriteLine($"[LoggingService] Using APPDATA log directory: {StorageConstants.LogsDirectory}");
        return StorageConstants.LogsDirectory;
    }

    /// <summary>
    /// Try to find the project's Logs directory
    /// Returns null if project root cannot be determined
    /// </summary>
    private string? TryGetProjectLogDirectory()
    {
        try
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            
            // Handle single-file publish (Location is empty)
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                assemblyLocation = AppContext.BaseDirectory;
            }

            var binFolder = Path.GetDirectoryName(assemblyLocation);
            
            // Navigate up from bin/x64/Debug/net10.0-windows... to project root
            // Structure: Project/bin/x64/Debug/net10.0-windows/...
            var current = binFolder;
            
            for (int i = 0; i < 6 && current != null; i++)
            {
                var potentialProjectFile = Path.Combine(current, "MyShop.Client.csproj");
                if (File.Exists(potentialProjectFile))
                {
                    return Path.Combine(current, "Logs");
                }
                current = Directory.GetParent(current)?.FullName;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Initialize Serilog with rolling file sinks
    /// Must be called before using any logging methods
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            var appLogPath = Path.Combine(_logDirectory, "app-.log");
            var errorLogPath = Path.Combine(_logDirectory, "errors-.log");

            // Delete existing log files for today (fresh start each run)
            try
            {
                var today = DateTime.Now.ToString("yyyyMMdd");
                var existingAppLog = Path.Combine(_logDirectory, $"app-{today}.log");
                var existingErrorLog = Path.Combine(_logDirectory, $"errors-{today}.log");
                
                if (File.Exists(existingAppLog))
                    File.Delete(existingAppLog);
                if (File.Exists(existingErrorLog))
                    File.Delete(existingErrorLog);
            }
            catch { /* Ignore deletion errors */ }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose() // Log EVERYTHING (Verbose = most detailed)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Include Microsoft logs too
                .MinimumLevel.Override("System", LogEventLevel.Information) // Include System logs too
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MyShop2025")
                .Enrich.WithProperty("Environment", System.Diagnostics.Debugger.IsAttached ? "Development" : "Production")
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("UserName", Environment.UserName)
                
                // Write all logs to daily rolling file (1 file per day, overwrite on restart)
                .WriteTo.File(
                    appLogPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 50_000_000, // 50MB per file
                    shared: true, // Allow multiple processes to write
                    flushToDiskInterval: TimeSpan.FromSeconds(1), // Flush every second
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                )
                
                // Write errors to separate file (with full exception details)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(
                        errorLogPath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                    )
                )
                
                // DO NOT use .WriteTo.Debug() here! It will cause infinite loop with Trace Listener
                // The Trace Listener already captures everything and writes to file
                
                .CreateLogger();

            Log.Information("═══════════════════════════════════════════════════════");
            Log.Information("MyShop2025 Logging Service Initialized");
            Log.Information("Log Directory: {LogDirectory}", _logDirectory);
            Log.Information("Session Start: {SessionStart}", DateTime.Now);
            Log.Information("═══════════════════════════════════════════════════════");

            _isInitialized = true;

            // Add Trace Listener to capture ALL Debug.WriteLine() and Trace.WriteLine() calls
            // Note: Debug.WriteLine internally calls Trace.WriteLine, so this captures both
            var listener = new DebugToSerilogListener();
            System.Diagnostics.Trace.Listeners.Add(listener);
            
            Log.Information("Debug Trace Listener installed - capturing all Debug.WriteLine() calls");

            // Cleanup old logs
            CleanupOldLogs();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize Serilog: {ex.Message}");
            throw;
        }
    }

    #region Core Logging Methods

    /// <summary>
    /// Log debug message (lowest level - for development only)
    /// </summary>
    public void Debug(
        string message,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "")
    {
        var sourceContext = GetSourceContext(file, caller);
        Log.ForContext("SourceContext", sourceContext)
           .Debug(message);
    }

    /// <summary>
    /// Log informational message (normal flow)
    /// </summary>
    public void Information(
        string message,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "")
    {
        var sourceContext = GetSourceContext(file, caller);
        Log.ForContext("SourceContext", sourceContext)
           .Information(message);
    }

    /// <summary>
    /// Log warning message (potential issues)
    /// </summary>
    public void Warning(
        string message,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "")
    {
        var sourceContext = GetSourceContext(file, caller);
        Log.ForContext("SourceContext", sourceContext)
           .Warning(message);
    }

    /// <summary>
    /// Log error message (failures that don't crash app)
    /// </summary>
    public void Error(
        string message,
        Exception? exception = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "")
    {
        var sourceContext = GetSourceContext(file, caller);
        
        if (exception != null)
        {
            // Log with full exception details
            Log.ForContext("SourceContext", sourceContext)
               .ForContext("ExceptionType", exception.GetType().FullName)
               .ForContext("ExceptionMessage", exception.Message)
               .ForContext("HResult", $"0x{exception.HResult:X8}")
               .Error(exception, message);
            
            // Also log to Debug output for immediate visibility
            System.Diagnostics.Debug.WriteLine($"[ERROR] {sourceContext}: {message}");
            System.Diagnostics.Debug.WriteLine($"  Exception: {exception.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"  Message: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"  HResult: 0x{exception.HResult:X8}");
            if (exception.StackTrace != null)
            {
                System.Diagnostics.Debug.WriteLine($"  StackTrace:\n{exception.StackTrace}");
            }
            if (exception.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  InnerException: {exception.InnerException.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"  InnerMessage: {exception.InnerException.Message}");
            }
        }
        else
        {
            Log.ForContext("SourceContext", sourceContext)
               .Error(message);
        }
    }

    /// <summary>
    /// Log fatal error (critical failures that may crash app)
    /// </summary>
    public void Fatal(
        string message,
        Exception? exception = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "")
    {
        var sourceContext = GetSourceContext(file, caller);
        
        if (exception != null)
        {
            // Log with full exception details
            Log.ForContext("SourceContext", sourceContext)
               .ForContext("ExceptionType", exception.GetType().FullName)
               .ForContext("ExceptionMessage", exception.Message)
               .ForContext("HResult", $"0x{exception.HResult:X8}")
               .Fatal(exception, message);
            
            // Also log to Debug output for immediate visibility
            System.Diagnostics.Debug.WriteLine($"[FATAL] {sourceContext}: {message}");
            System.Diagnostics.Debug.WriteLine($"  Exception: {exception.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"  Message: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"  HResult: 0x{exception.HResult:X8}");
            if (exception.StackTrace != null)
            {
                System.Diagnostics.Debug.WriteLine($"  StackTrace:\n{exception.StackTrace}");
            }
            if (exception.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  InnerException: {exception.InnerException.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"  InnerMessage: {exception.InnerException.Message}");
            }
            
            // Force immediate flush for fatal errors
            Log.CloseAndFlush();
        }
        else
        {
            Log.ForContext("SourceContext", sourceContext)
               .Fatal(message);
            Log.CloseAndFlush();
        }
    }

    #endregion

    #region Specialized Logging

    /// <summary>
    /// Log navigation event with structured data
    /// </summary>
    public void LogNavigation(
        string fromPage,
        string toPage,
        object? parameter = null,
        bool isSuccess = true,
        string? errorMessage = null)
    {
        if (isSuccess)
        {
            Log.ForContext("SourceContext", "Navigation")
               .ForContext("FromPage", fromPage)
               .ForContext("ToPage", toPage)
               .ForContext("Parameter", parameter?.GetType().Name ?? "null")
               .Information("Navigation: {FromPage} → {ToPage}", fromPage, toPage);
        }
        else
        {
            Log.ForContext("SourceContext", "Navigation")
               .ForContext("FromPage", fromPage)
               .ForContext("ToPage", toPage)
               .ForContext("ErrorMessage", errorMessage)
               .Error("Navigation Failed: {FromPage} → {ToPage}: {ErrorMessage}", fromPage, toPage, errorMessage);
        }
    }

    /// <summary>
    /// Log API/Repository operation
    /// </summary>
    public void LogApiCall(
        string operation,
        string endpoint,
        bool isSuccess,
        long? durationMs = null,
        string? errorMessage = null)
    {
        if (isSuccess)
        {
            Log.ForContext("SourceContext", "API")
               .ForContext("Operation", operation)
               .ForContext("Endpoint", endpoint)
               .ForContext("DurationMs", durationMs)
               .Information("API Call: {Operation} → {Endpoint} ({DurationMs}ms)", operation, endpoint, durationMs);
        }
        else
        {
            Log.ForContext("SourceContext", "API")
               .ForContext("Operation", operation)
               .ForContext("Endpoint", endpoint)
               .ForContext("ErrorMessage", errorMessage)
               .Error("API Call Failed: {Operation} → {Endpoint}: {ErrorMessage}", operation, endpoint, errorMessage);
        }
    }

    /// <summary>
    /// Log authentication event
    /// </summary>
    public void LogAuth(
        string action,
        string? username = null,
        bool isSuccess = true,
        string? errorMessage = null)
    {
        if (isSuccess)
        {
            Log.ForContext("SourceContext", "Auth")
               .ForContext("Action", action)
               .ForContext("Username", username ?? "Unknown")
               .Information("Auth: {Action} for {Username}", action, username ?? "Unknown");
        }
        else
        {
            Log.ForContext("SourceContext", "Auth")
               .ForContext("Action", action)
               .ForContext("Username", username ?? "Unknown")
               .ForContext("ErrorMessage", errorMessage)
               .Warning("Auth Failed: {Action} for {Username}: {ErrorMessage}", action, username ?? "Unknown", errorMessage);
        }
    }

    /// <summary>
    /// Log data operation (CRUD)
    /// </summary>
    public void LogDataOperation(
        string operation,
        string entity,
        int? count = null,
        bool isSuccess = true,
        string? errorMessage = null)
    {
        if (isSuccess)
        {
            Log.ForContext("SourceContext", "Data")
               .ForContext("Operation", operation)
               .ForContext("Entity", entity)
               .ForContext("Count", count)
               .Information("Data: {Operation} {Entity} ({Count} items)", operation, entity, count);
        }
        else
        {
            Log.ForContext("SourceContext", "Data")
               .ForContext("Operation", operation)
               .ForContext("Entity", entity)
               .ForContext("ErrorMessage", errorMessage)
               .Error("Data Operation Failed: {Operation} {Entity}: {ErrorMessage}", operation, entity, errorMessage);
        }
    }

    /// <summary>
    /// Log performance metric
    /// </summary>
    public void LogPerformance(
        string operation,
        long milliseconds,
        string? context = null)
    {
        Log.ForContext("SourceContext", "Performance")
           .ForContext("Operation", operation)
           .ForContext("DurationMs", milliseconds)
           .ForContext("Context", context ?? "N/A")
           .Information("Performance: {Operation} took {DurationMs}ms", operation, milliseconds);
    }

    /// <summary>
    /// Log ViewModel lifecycle event
    /// </summary>
    public void LogViewModelEvent(
        string viewModelName,
        string eventType,
        string? details = null)
    {
        Log.ForContext("SourceContext", "ViewModel")
           .ForContext("ViewModel", viewModelName)
           .ForContext("EventType", eventType)
           .ForContext("Details", details ?? "N/A")
           .Debug("ViewModel Event: {ViewModel}.{EventType}", viewModelName, eventType);
    }

    #endregion

    #region Helper Methods

    private string GetSourceContext(string filePath, string methodName)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        return $"{fileName}.{methodName}";
    }

    private void CleanupOldLogs()
    {
        try
        {
            var directory = new DirectoryInfo(_logDirectory);
            if (!directory.Exists) return;

            var cutoffDate = DateTime.Now.AddDays(-30);
            var oldFiles = directory.GetFiles("*.log")
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
                Log.Information("Cleaned up {DeletedCount} old log files (older than 30 days)", deletedCount);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Get log directory path
    /// </summary>
    public string GetLogDirectory() => _logDirectory;

    /// <summary>
    /// Check if logging is initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Log.CloseAndFlush();
    }

    #endregion

    #region Debug Trace Listener

    /// <summary>
    /// Custom TraceListener that forwards all Debug.WriteLine() calls to Serilog FILE ONLY
    /// This captures ALL debug messages from repositories, facades, viewmodels, etc.
    /// IMPORTANT: Does NOT write back to Debug output to avoid infinite loop
    /// </summary>
    private class DebugToSerilogListener : System.Diagnostics.TraceListener
    {
        private static bool _isLogging = false; // Prevent re-entrancy
        
        public override void Write(string? message)
        {
            if (_isLogging || string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                _isLogging = true;
                
                // Write ONLY to file sinks (not Debug output)
                using (Serilog.Context.LogContext.PushProperty("SkipDebugOutput", true))
                {
                    Log.Debug(message);
                }
            }
            finally
            {
                _isLogging = false;
            }
        }

        public override void WriteLine(string? message)
        {
            if (_isLogging || string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                _isLogging = true;
                
                // Parse message to determine appropriate log level
                using (Serilog.Context.LogContext.PushProperty("SkipDebugOutput", true))
                {
                    if (message.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("❌", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("Exception", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Error(message);
                    }
                    else if (message.Contains("WARNING", StringComparison.OrdinalIgnoreCase) ||
                             message.Contains("⚠️", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Warning(message);
                    }
                    else if (message.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                             message.Contains("✅", StringComparison.OrdinalIgnoreCase) ||
                             message.Contains("Loaded", StringComparison.OrdinalIgnoreCase) ||
                             message.Contains("success", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information(message);
                    }
                    else
                    {
                        Log.Debug(message);
                    }
                }
            }
            finally
            {
                _isLogging = false;
            }
        }
    }

    #endregion
}
