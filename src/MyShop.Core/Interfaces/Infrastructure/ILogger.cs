namespace MyShop.Core.Interfaces.Infrastructure;

/// <summary>
/// Interface for logging abstraction
/// Allows swapping implementation: File logger, Debug logger, Cloud logger, etc.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Log information message
    /// </summary>
    void LogInfo(string message);

    /// <summary>
    /// Log warning message
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Log error message
    /// </summary>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Log debug message
    /// </summary>
    void LogDebug(string message);

    /// <summary>
    /// Log with custom severity level
    /// </summary>
    void Log(LogLevel level, string message, Exception? exception = null);
}

/// <summary>
/// Log severity levels
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}
