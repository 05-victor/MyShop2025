namespace MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Common;

/// <summary>
/// Interface for logging abstraction
/// Allows swapping implementation: File logger, Debug logger, Cloud logger, etc.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Log information message
    /// </summary>
    Task<Result<Unit>> LogInfo(string message);

    /// <summary>
    /// Log warning message
    /// </summary>
    Task<Result<Unit>> LogWarning(string message);

    /// <summary>
    /// Log error message
    /// </summary>
    Task<Result<Unit>> LogError(string message, Exception? exception = null);

    /// <summary>
    /// Log debug message
    /// </summary>
    Task<Result<Unit>> LogDebug(string message);

    /// <summary>
    /// Log with custom severity level
    /// </summary>
    Task<Result<Unit>> Log(LogLevel level, string message, Exception? exception = null);
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
