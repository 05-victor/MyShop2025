using System.Diagnostics;

namespace MyShop.Plugins.Repositories.Base;

/// <summary>
/// Base repository class providing common logging utilities.
/// Repositories can optionally extend this for consistent logging patterns.
/// Does NOT enforce any specific error handling pattern - each repository 
/// maintains its own try-catch logic as per existing codebase conventions.
/// </summary>
public abstract class BaseRepository
{
    /// <summary>
    /// Gets the repository name for logging purposes.
    /// </summary>
    protected string RepositoryName => GetType().Name;

    /// <summary>
    /// Logs debug information with repository context.
    /// </summary>
    /// <param name="message">The message to log</param>
    protected void LogDebug(string message)
    {
        Debug.WriteLine($"[{RepositoryName}] {message}");
    }

    /// <summary>
    /// Logs debug information with operation context.
    /// </summary>
    /// <param name="operation">The operation name</param>
    /// <param name="message">The message to log</param>
    protected void LogDebug(string operation, string message)
    {
        Debug.WriteLine($"[{RepositoryName}] {operation}: {message}");
    }

    /// <summary>
    /// Logs error information with repository context.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="ex">Optional exception</param>
    protected void LogError(string message, Exception? ex = null)
    {
        Debug.WriteLine($"[{RepositoryName}] ERROR: {message}");
        if (ex != null)
        {
            Debug.WriteLine($"[{RepositoryName}] Exception: {ex.Message}");
#if DEBUG
            Debug.WriteLine($"[{RepositoryName}] StackTrace: {ex.StackTrace}");
#endif
        }
    }

    /// <summary>
    /// Logs error information with operation context.
    /// </summary>
    /// <param name="operation">The operation name</param>
    /// <param name="message">The error message</param>
    /// <param name="ex">Optional exception</param>
    protected void LogError(string operation, string message, Exception? ex = null)
    {
        Debug.WriteLine($"[{RepositoryName}] {operation} ERROR: {message}");
        if (ex != null)
        {
            Debug.WriteLine($"[{RepositoryName}] Exception: {ex.Message}");
#if DEBUG
            Debug.WriteLine($"[{RepositoryName}] StackTrace: {ex.StackTrace}");
#endif
        }
    }

    /// <summary>
    /// Logs a warning message with repository context.
    /// </summary>
    /// <param name="message">The warning message</param>
    protected void LogWarning(string message)
    {
        Debug.WriteLine($"[{RepositoryName}] WARNING: {message}");
    }

    /// <summary>
    /// Logs a warning message with operation context.
    /// </summary>
    /// <param name="operation">The operation name</param>
    /// <param name="message">The warning message</param>
    protected void LogWarning(string operation, string message)
    {
        Debug.WriteLine($"[{RepositoryName}] {operation} WARNING: {message}");
    }
}
